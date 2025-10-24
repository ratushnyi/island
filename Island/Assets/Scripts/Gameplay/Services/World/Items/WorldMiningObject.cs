using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.HUD;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using TendedTarsier.Core.Services.Input;
using UniRx;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.World.Items
{
    public class WorldMiningObject : WorldObjectBase
    {
        [SerializeField, SerializedDictionary("Health", "ViewObject")]
        private SerializedDictionary<int, GameObject> _healthView;
        [SerializeField, SerializedDictionary("Tool", "Damage")]
        private SerializedDictionary<InventoryItemType, int> _tools;
        [SerializeField] private float _duration;
        [SerializeField] private ItemEntity _resultItem;

        [Inject] private InventoryService _inventoryService;
        [Inject] private InputService _inputService;
        [Inject] private AimService _aimService;
        [Inject] private HUDService _hudService;

        private UniTaskCompletionSource _completionSource;

        public override string Name => Type.ToString();

        protected override void UpdateView(int health)
        {
            bool selected = false;
            foreach (var view in _healthView)
            {
                if (!selected && health >= view.Key)
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
            if (_completionSource?.Task.Status == UniTaskStatus.Pending)
            {
                _completionSource.TrySetResult();
            }
        }

        public override async UniTask<bool> Perform(bool isJustUsed)
        {
            if (_completionSource?.Task.Status == UniTaskStatus.Pending)
            {
                return false;
            }

            if (Health.Value <= 0)
            {
                return false;
            }

            if (!Check())
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
            
            _inventoryService.TryCollect(_resultItem);

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

            var newHealth = Health.Value - damage;
            OnHealthChanged_ServerRpc(newHealth);

            if (Health.Value <= 0)
            {
                Despawn_ServerRpc();
            }

            return true;
        }

        private bool Check()
        {
            if (!_inventoryService.IsEnoughSpace(_resultItem))
            {
                return false;
            }

            return true;
        }

        private async UniTask<bool> TryHold()
        {
            _completionSource = new UniTaskCompletionSource();
            var onPlayerExit = _aimService.TargetObject.SkipLatestValueOnSubscribe().First().ToUniTask();
            var onStopInteract = _inputService.OnInteractButtonCanceled.First().ToUniTask();
            var progressBar = _hudService.ProgressBar.Show(_duration);
            var holdTask = UniTask.WhenAny(progressBar, onStopInteract, onPlayerExit, _completionSource.Task);
            var holdResult = await holdTask == 0;

            _hudService.ProgressBar.Hide();
            _completionSource.TrySetResult();

            return holdResult;
        }
    }
}