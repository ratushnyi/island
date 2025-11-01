using System;
using Cysharp.Threading.Tasks;
using Island.Common.Services;
using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Panels.HUD;
using Island.Gameplay.Profiles.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using Island.Gameplay.Services.World.Items;
using JetBrains.Annotations;
using TendedTarsier.Core.Panels;
using TendedTarsier.Core.Services;
using UniRx;
using Zenject;

namespace Island.Gameplay.Services.Inventory
{
    [UsedImplicitly]
    public class InventoryService : ServiceBase, INetworkInitialize
    {
        private PanelLoader<HUDPanel> _hudPanel;
        private InventoryConfig _inventoryConfig;
        private InventoryProfile _inventoryProfile;
        public InventoryItemType SelectedItem => _inventoryProfile.SelectedItem.Value;

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

        public void OnNetworkInitialize()
        {
            SubscribeOnItemsChanged();
            SubscribeOnItemSelected();
        }

        private void SubscribeOnItemSelected()
        {
            _inventoryProfile.SelectedItem.Subscribe(type =>
            {
                if (type == InventoryItemType.None)
                {
                    _hudPanel.Instance.SelectedItem.SetEmpty();
                    return;
                }

                _hudPanel.Instance.SelectedItem.SetItem(_inventoryConfig[type], _inventoryProfile.InventoryItems[type]);
            }).AddTo(CompositeDisposable);
        }

        private void SubscribeOnItemsChanged()
        {
            foreach (var item in _inventoryProfile.InventoryItems)
            {
                subscribe(item.Key, item.Value);
            }

            _inventoryProfile.InventoryItems.ObserveAdd().Subscribe(t => subscribe(t.Key, t.Value)).AddTo(CompositeDisposable);

            void subscribe(InventoryItemType type, ReactiveProperty<int> value)
            {
                var disposable = value.SkipLatestValueOnSubscribe().Subscribe(count => onCountChanged(count, type))
                    .AddTo(CompositeDisposable);

                _inventoryProfile.InventoryItems.ObserveRemove()
                    .Where(t => t.Key == type)
                    .First()
                    .Subscribe(_ => onItemRemoved(disposable));
            }

            void onCountChanged(int count, InventoryItemType type)
            {
                if (count == 0)
                {
                    _inventoryProfile.InventoryItems.Remove(type);
                    if (_inventoryProfile.SelectedItem.Value == type)
                    {
                        _inventoryProfile.SelectedItem.Value = InventoryItemType.Hand;
                    }
                }

                _inventoryProfile.Save();
            }

            void onItemRemoved(IDisposable disposable)
            {
                CompositeDisposable.Remove(disposable);
            }
        }

        public bool IsEnough(ItemEntity itemEntity, int multiplier = 1) => IsEnough(itemEntity.Type, itemEntity.Count * multiplier);

        public bool IsEnough(InventoryItemType type, int count)
        {
            if (_inventoryProfile.InventoryItems.TryGetValue(type, out var item))
            {
                if (item.Value >= count)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryRemove(ItemEntity itemEntity, Func<UniTask> beforeItemRemove = null) => TryRemove(itemEntity.Type, itemEntity.Count, beforeItemRemove);

        public bool TryRemove(InventoryItemType type, int count, Func<UniTask> beforeItemRemove = null)
        {
            if (_inventoryProfile.InventoryItems.TryGetValue(type, out var item))
            {
                if (item.Value >= count)
                {
                    removeExistItem().Forget();
                    return true;
                }
            }

            return false;

            async UniTask removeExistItem()
            {
                if (beforeItemRemove != null)
                {
                    await beforeItemRemove.Invoke();
                }

                item.Value -= count;
            }
        }

        public bool IsEnoughSpace(ItemEntity itemEntity) => IsEnoughSpace(itemEntity.Type);

        public bool IsEnoughSpace(InventoryItemType type)
        {
            if (!_inventoryProfile.InventoryItems.TryGetValue(type, out var item))
            {
                if (_inventoryProfile.InventoryItems.Count >= _inventoryConfig.InventoryCapacity)
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryCollect(ItemEntity itemEntity, Func<UniTask> beforeItemAdd = null) => TryCollect(itemEntity.Type, itemEntity.Count, beforeItemAdd);

        public bool TryCollect(InventoryItemType type, int count, Func<UniTask> beforeItemAdd = null)
        {
            if (type == InventoryItemType.None)
            {
                return false;
            }

            if (_inventoryProfile.InventoryItems.TryGetValue(type, out var existItem))
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

                existItem.Value += count;
            }

            async UniTask addNewItem()
            {
                if (beforeItemAdd != null)
                {
                    await beforeItemAdd.Invoke();
                }

                if (_inventoryProfile.InventoryItems.TryGetValue(type, out existItem))
                {
                    existItem.Value += count;
                }
                else
                {
                    var property = new ReactiveProperty<int>(count);
                    _inventoryProfile.InventoryItems.Add(type, property);
                }
            }
        }

        public async UniTask<bool> PerformSelectedItem(bool isJustUsed, float deltaTime)
        {
            var item = _inventoryProfile.SelectedItem.Value;
            var itemModel = _inventoryConfig[item];
            var result = await itemModel.Perform(isJustUsed, deltaTime);

            if (itemModel.IsDisposable && result)
            {
                TryRemove(item, 1);
            }

            return result;
        }
    }
}