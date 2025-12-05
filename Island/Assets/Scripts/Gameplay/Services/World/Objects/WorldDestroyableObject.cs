using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.HUD;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using TendedTarsier.Core.Services.Input;
using TendedTarsier.Core.Utilities.Extensions;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.World.Objects
{
    public class WorldDestroyableObject : WorldObjectBase
    {
        [SerializeField, SerializedDictionary("Health", "ViewObject")]
        private SerializedDictionary<int, GameObject> _healthView;

        [SerializeField, SerializedDictionary("Tool", "Damage")]
        private SerializedDictionary<InventoryItemType, int> _tools;

        [SerializeField] private float _duration;
        [SerializeField] private ItemStack _dropItem;

        [Inject] private InputService _inputService;
        [Inject] private AimService _aimService;
        [Inject] private HUDService _hudService;
        [Inject] private InventoryService _inventoryService;
        [Inject] private WorldService _worldService;

        private readonly NetworkVariable<bool> _isPerformingInProgress = new();
        private readonly NetworkVariable<int> _health = new();

        public override string Name => Type.ToString();

        public void InitHealth(int health)
        {
            _health.Value = health;
        }

        public override void OnNetworkSpawn()
        {
            _health.AsObservable().Subscribe(UpdateView).AddTo(this);
            if (IsClient)
            {
                UpdateView(_health.Value);
            }
        }

        private void UpdateView(int health)
        {
            bool selected = false;
            foreach (var view in _healthView)
            {
                if (!selected && health > view.Key)
                {
                    selected = true;
                    view.Value.SetActive(true);
                    continue;
                }

                view.Value.SetActive(false);
            }
        }

        public override async UniTask<bool> Perform(bool isJustUsed, float deltaTime)
        {
            if (_isPerformingInProgress.Value)
            {
                return false;
            }

            if (_health.Value <= 0)
            {
                return false;
            }

            if (!await TryHold())
            {
                return false;
            }

            return TryDamage();
        }

        private bool TryDamage()
        {
            _tools.TryGetValue(InventoryItemType.Hand, out var damage);
            _tools.TryGetValue(_inventoryService.SelectedItem.Type, out damage);
            if (damage == 0)
            {
                return false;
            }

            DoDamage_ServerRpc(_health.Value - damage);

            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        private void DoDamage_ServerRpc(int value)
        {
            _health.Value = value;
            _worldService.UpdateHealth(this, _health.Value);

            var position = transform.position + Vector3.up + Vector3.up;
            for (int i = 0; i < _dropItem.Count; i++)
            {
                _worldService.Spawn(position, WorldObjectType.Collectable, _dropItem.Type);
            }

            if (_health.Value <= 0)
            {
                Despawn_ServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetPerformingInProgress_ServerRpc(bool value)
        {
            _isPerformingInProgress.Value = value;
        }

        private async UniTask<bool> TryHold()
        {
            SetPerformingInProgress_ServerRpc(true);
            var onPlayerExit = _aimService.TargetObject.SkipLatestValueOnSubscribe().First().ToUniTask();
            var onStopInteract = _inputService.OnInteractButtonCanceled.First().ToUniTask();
            var progressBar = _hudService.ProgressBar.Show(_duration);
            var onStopPerforming = UniTask.WaitWhile(() => _isPerformingInProgress.Value);
            var holdTask = UniTask.WhenAny(progressBar, onStopInteract, onPlayerExit, onStopPerforming);
            var holdResult = await holdTask == 0;

            _hudService.ProgressBar.Hide();
            SetPerformingInProgress_ServerRpc(false);

            return holdResult;
        }
    }
}