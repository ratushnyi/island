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
        [SerializeField] private ItemEntity _dropItem;

        [Inject] private InputService _inputService;
        [Inject] private AimService _aimService;
        [Inject] private HUDService _hudService;
        [Inject] private InventoryService _inventoryService;
        [Inject] private WorldService _worldService;

        private readonly NetworkVariable<bool> _isPerforming = new();
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

        public void Reset()
        {
            _isPerforming.Value = false;
        }

        public override async UniTask<bool> Perform(bool isJustUsed, float deltaTime)
        {
            if (_isPerforming.Value)
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

            if (!TryDamage())
            {
                return false;
            }

            var position = transform.position + Vector3.up + Vector3.up;
            _worldService.SpawnCollectableItem(position, _dropItem);

            return true;
        }

        private bool TryDamage()
        {
            _tools.TryGetValue(InventoryItemType.Hand, out var damage);
            _tools.TryGetValue(_inventoryService.SelectedItem, out damage);
            if (damage == 0)
            {
                return false;
            }

            OnHealthChanged_ServerRpc(_health.Value - damage);

            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        private void OnHealthChanged_ServerRpc(int value)
        {
            _health.Value = value;
            _worldService.UpdateHealth(this, _health.Value);
            
            if (_health.Value <= 0)
            {
                Despawn_ServerRpc();
            }
        }

        private async UniTask<bool> TryHold()
        {
            _isPerforming.Value = true;
            var onPlayerExit = _aimService.TargetObject.SkipLatestValueOnSubscribe().First().ToUniTask();
            var onStopInteract = _inputService.OnInteractButtonCanceled.First().ToUniTask();
            var progressBar = _hudService.ProgressBar.Show(_duration);
            var onStopPerforming = UniTask.WaitWhile(() => _isPerforming.Value);
            var holdTask = UniTask.WhenAny(progressBar, onStopInteract, onPlayerExit, onStopPerforming);
            var holdResult = await holdTask == 0;

            _hudService.ProgressBar.Hide();
            _isPerforming.Value = false;

            return holdResult;
        }
    }
}