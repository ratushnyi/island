using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Panels.HUD;
using Island.Gameplay.Profiles.Inventory;
using JetBrains.Annotations;
using TendedTarsier.Core.Panels;
using TendedTarsier.Core.Services;
using UniRx;
using Zenject;

namespace Island.Gameplay.Services.Inventory
{
    [UsedImplicitly]
    public class InventoryService : ServiceBase, IInitializable
    {
        private PanelLoader<HUDPanel> _hudPanel;
        private InventoryConfig _inventoryConfig;
        private InventoryProfile _inventoryProfile;

        [Inject]
        private void Construct(
            InventoryProfile inventoryProfile,
            InventoryConfig inventoryConfig,
            PanelLoader<HUDPanel> hudPanel)
        {
            _inventoryProfile = inventoryProfile;
            _inventoryConfig = inventoryConfig;
            _hudPanel = hudPanel;
        }

        public void Initialize()
        {
            SubscribeOnItemsChanged();
            SubscribeOnItemDropped();
            SubscribeOnItemSelected();
        }

        private void SubscribeOnItemSelected()
        {
            _inventoryProfile.SelectedItem.Subscribe(itemId =>
            {
                if (string.IsNullOrEmpty(itemId))
                {
                    _hudPanel.Instance.SelectedItem.SetEmpty();
                    return;
                }

                _hudPanel.Instance.SelectedItem.SetItem(_inventoryConfig[itemId], _inventoryProfile.InventoryItems[itemId]);
            }).AddTo(CompositeDisposable);
        }

        private void SubscribeOnItemDropped()
        {
            _hudPanel.Instance.SelectedItem.OnButtonClicked.Subscribe(Drop).AddTo(CompositeDisposable);
        }

        private void SubscribeOnItemsChanged()
        {
            foreach (var item in _inventoryProfile.InventoryItems)
            {
                subscribe(item.Key, item.Value);
            }

            _inventoryProfile.InventoryItems.ObserveAdd().Subscribe(t => subscribe(t.Key, t.Value)).AddTo(CompositeDisposable);

            void subscribe(string id, ReactiveProperty<int> value)
            {
                var disposable = value.SkipLatestValueOnSubscribe().Subscribe(count => onCountChanged(count, id))
                    .AddTo(CompositeDisposable);

                _inventoryProfile.InventoryItems.ObserveRemove()
                    .Where(t => t.Key == id)
                    .First()
                    .Subscribe(_ => onItemRemoved(disposable));
            }

            void onCountChanged(int count, string id)
            {
                if (count == 0)
                {
                    _inventoryProfile.InventoryItems.Remove(id);
                    if (_inventoryProfile.SelectedItem.Value == id)
                    {
                        _inventoryProfile.SelectedItem.Value = null;
                    }
                }

                _inventoryProfile.Save();
            }

            void onItemRemoved(IDisposable disposable)
            {
                CompositeDisposable.Remove(disposable);
            }
        }

        private void Drop(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return;
            }

            _inventoryProfile.InventoryItems[itemId].Value--;
        }

        public bool TryPut(string id, int count, Func<UniTask> beforeItemAdd = null)
        {
            var existItem = _inventoryProfile.InventoryItems.FirstOrDefault(t => t.Key == id);
            if (existItem.Key != null)
            {
                addExistItem().Forget();
                return true;
            }

            if (_inventoryProfile.InventoryItems.Count >= _inventoryConfig.InventoryCapacity)
            {
                return false;
            }

            addNewItem().Forget();
            return true;

            async UniTask addExistItem()
            {
                if (beforeItemAdd != null)
                {
                    await beforeItemAdd.Invoke();
                }
                existItem.Value.Value += count;
            }

            async UniTask addNewItem()
            {
                if (beforeItemAdd != null)
                {
                    await beforeItemAdd.Invoke();
                }
                existItem = _inventoryProfile.InventoryItems.FirstOrDefault(t => t.Key == id);
                if (existItem.Key != null)
                {
                    existItem.Value.Value += count;
                }
                else
                {
                    var property = new ReactiveProperty<int>(count);
                    _inventoryProfile.InventoryItems.Add(id, property);
                }
            }
        }

        public bool TryPut(WorldItemObject worldItem)
        {
            return TryPut(worldItem.ItemEntity.Id, worldItem.ItemEntity.Count, () => UniTask.CompletedTask);
        }

        public async UniTask<bool> Perform()
        {
            var result = false;
            var item = _inventoryProfile.SelectedItem.Value;
            if (!string.IsNullOrEmpty(item))
            {
                var itemModel = _inventoryConfig[item];
                result = await itemModel.Perform();

                if (itemModel.IsCountable && result)
                {
                    _inventoryProfile.InventoryItems[item].Value--;
                }
            }

            return result;
        }
    }
}