using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Island.Common;
using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Panels.HUD;
using Island.Gameplay.Profiles.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using Island.Gameplay.Services.Server;
using JetBrains.Annotations;
using TendedTarsier.Core.Panels;
using TendedTarsier.Core.Services;
using UniRx;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.Inventory
{
    [UsedImplicitly]
    public class InventoryService : ServiceBase, IServerInitialize, IPerformable
    {
        private PanelLoader<HUDPanel> _hudPanel;
        private InventoryConfig _inventoryConfig;
        private InventoryProfile _inventoryProfile;
        public ItemModel SelectedItem => _inventoryConfig[_inventoryProfile.SelectedItemType];

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
            SubscribeOnItemSelected();
        }

        private void SubscribeOnItemSelected()
        {
            _inventoryProfile.SelectedItem.Subscribe(index =>
            {
                _hudPanel.Instance.SelectedItem.SetItem(_inventoryProfile.InventoryItems, _inventoryConfig[_inventoryProfile.SelectedItemType], index);
            }).AddTo(CompositeDisposable);
        }

        public bool IsEnough(ItemStack itemStack, int multiplier = 1) => IsEnough(itemStack.Type, itemStack.Count * multiplier);

        public bool IsEnough(InventoryItemType type, int count)
        {
            var stack = _inventoryProfile[type];
            if (stack.Type != InventoryItemType.None)
            {
                if (stack.Count >= count)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryRemove(ItemStack itemStack) => TryRemove(itemStack.Type, itemStack.Count);

        public bool TryRemove(InventoryItemType type, int count)
        {
            var stack = _inventoryProfile[type];
            if (stack.Type != InventoryItemType.None)
            {
                if (stack.Count >= count)
                {
                    removeItem();
                    return true;
                }
            }

            return false;

            void removeItem()
            {
                var endValue = stack.Count - count;
                var type = stack.Type;

                if (endValue == 0)
                {
                    type = InventoryItemType.None;
                    
                    if (_inventoryProfile.SelectedItemType == stack.Type)
                    {
                        _inventoryProfile.SelectedItem.Value = 0;
                    }
                }

                _inventoryProfile.InventoryItems.Populate(stack, new ItemStack(type, endValue));
            }
        }

        public bool TryRemove(int stackIndex, int count)
        {
            var stack = _inventoryProfile.InventoryItems[stackIndex];
            var endValue = stack.Count - count;
            switch (endValue)
            {
                case > 0:
                    _inventoryProfile.InventoryItems.Populate(stack, new ItemStack(stack.Type, endValue));
                    return true;
                case 0:
                    _inventoryProfile.InventoryItems.Populate(stack, new ItemStack(InventoryItemType.None, endValue));
                    return true;
                default:
                    return false;
            }
        }

        public bool IsEnoughSpace(ItemStack itemStack) => IsEnoughSpace(itemStack.Type, itemStack.Count);

        public bool IsEnoughSpace(InventoryItemType type, int count)
        {
            var stackSize = _inventoryConfig[type].StackSize;
            var stack = _inventoryProfile.InventoryItems.FirstOrDefault(t => t.Type == type && t.Count < stackSize);
            while (count > 0 && stack.Type != InventoryItemType.None)
            {
                var stackCount = Mathf.Min(stackSize - stack.Count, count);
                count -= stackCount;
                if (count > 0)
                {
                    stack = _inventoryProfile.InventoryItems.FirstOrDefault(t => t.Type == type && t.Count < stackSize);
                }
            }

            while (count > 0 && _inventoryProfile.InventoryItems.Any(t => t.Type == InventoryItemType.None))
            {
                var stackCount = Mathf.Min(stackSize, count);
                count -= stackCount;
            }

            return count == 0;
        }

        public bool TryCollect(ItemStack itemStack) => TryCollect(itemStack.Type, itemStack.Count);

        public bool TryCollect(InventoryItemType type, int count)
        {
            if (!IsEnoughSpace(type, count))
            {
                return false;
            }
            
            var stackSize = _inventoryConfig[type].StackSize;
            var stack = _inventoryProfile.InventoryItems.FirstOrDefault(t => t.Type == type && t.Count < stackSize);
            
            while (count > 0 && stack.Type != InventoryItemType.None)
            {
                var stackCount = Mathf.Min(stackSize - stack.Count, count);
                _inventoryProfile.InventoryItems.Populate(stack, new ItemStack(stack.Type, stack.Count + stackCount));
                count -= stackCount;
                if (count > 0)
                {
                    stack = _inventoryProfile.InventoryItems.FirstOrDefault(t => t.Type == type && t.Count < stackSize);
                }
            }

            while (count > 0 && _inventoryProfile.InventoryItems.Any(t => t.Type == InventoryItemType.None))
            {
                var stackCount = Mathf.Min(stackSize, count);
                _inventoryProfile.InventoryItems.Populate(_inventoryProfile[InventoryItemType.None], new ItemStack(type, stackCount));
                count -= stackCount;
            }

            return true;
        }

        public async UniTask<bool> Perform(bool isJustUsed, float deltaTime)
        {
            var result = await SelectedItem.ItemEntity.Perform(isJustUsed, deltaTime);

            if (SelectedItem.IsDisposable && result)
            {
                TryRemove(SelectedItem.Type, 1);
            }

            return result;
        }
    }
}