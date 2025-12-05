using System;
using System.Linq;
using Island.Common;
using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Panels.Player.Inventory;
using Island.Gameplay.Profiles.Inventory;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using TendedTarsier.Core.Panels;
using TendedTarsier.Core.Utilities.Extensions;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Island.Gameplay.Services.World.Objects.UI
{
    public class WorldWarehousePopup : ResultPopupBase<(ItemStack[] input, ItemStack[] output)>
    {
        [SerializeField] private Button _toButton;
        [SerializeField] private Button _toAllButton;
        [SerializeField] private Button _fromButton;
        [SerializeField] private Button _fromAllButton;
        [SerializeField] private Transform _inventoryGridContainer;
        [SerializeField] private Transform _warehouseGridContainer;
        [SerializeField] private int _capacity = 20;

        [Inject] private InventoryConfig _inventoryConfig;
        [Inject] private InventoryProfile _inventoryProfile;
        [Inject] private InventoryService _inventoryService;
        [Inject] private EventSystem _eventSystem;
        private InventoryCellView[] _inventoryCellsList;
        private InventoryCellView[] _warehouseCellsList;
        private int _selectedCell;
        private WorldContainerBase _container;
        private InventoryCellView _selectedInventoryCell;
        private InventoryCellView _selectedWarehouseCell;

        [Inject]
        private void Initialize(WorldContainerBase container)
        {
            _container = container;
            SetButtonsInteractable(false, false);
            _toButton.OnClickAsObservable().Subscribe(_ => OnMoveButtonClicked(true, false)).AddTo(this);
            _toAllButton.OnClickAsObservable().Subscribe(_ => OnMoveButtonClicked(true, true)).AddTo(this);
            _fromButton.OnClickAsObservable().Subscribe(_ => OnMoveButtonClicked(false, false)).AddTo(this);
            _fromAllButton.OnClickAsObservable().Subscribe(_ => OnMoveButtonClicked(false, true)).AddTo(this);
            InitGrid(_inventoryGridContainer, _inventoryConfig.InventoryCapacity, _inventoryProfile.InventoryItems, out _inventoryCellsList, true);
            InitGrid(_warehouseGridContainer, _capacity, _container.ContainerArray.ToReactiveCollection(), out _warehouseCellsList, false);
        }

        private void OnMoveButtonClicked(bool isTo, bool isAll)
        {
            ItemStack stack;
            var count = 1;
            if (isTo)
            {
                stack = _inventoryCellsList[_selectedCell].Stack;
                if (isAll)
                {
                    count = stack.Count;
                }

                _inventoryService.TryRemove(_selectedCell, count);
            }
            else
            {
                stack = _warehouseCellsList[_selectedCell].Stack;
                if (isAll)
                {
                    count = stack.Count;
                }

                _inventoryService.TryCollect(stack.Type, count);
            }

            _container.ApplyContainer_ServerRpc(new ItemStack[] { new(stack.Type, isTo ? count : -count) });
            OnCellClicked(_selectedCell, isTo);
        }

        private void InitGrid(Transform container, int capacity, ReactiveCollection<ItemStack> items, out InventoryCellView[] cellList, bool isInventory)
        {
            cellList = new InventoryCellView[capacity];

            var validItems = items.Where(t => t.Type >= 0).ToList();
            for (var i = 0; i < capacity; i++)
            {
                cellList[i] = Instantiate(_inventoryConfig.InventoryCellView, container);
                cellList[i].OnButtonClicked.Subscribe(index => OnCellClicked(index, isInventory)).AddTo(this);

                if (validItems.Count > i)
                {
                    var item = validItems.ElementAt(i);
                    
                    cellList[i].SetItem(items, _inventoryConfig[item.Type], i);
                }
            }
        }

        private void OnCellClicked(int index, bool isInventory)
        {
            var cellsList = isInventory ? _inventoryCellsList : _warehouseCellsList;
            var isEmpty = cellsList[index].Stack.Type == InventoryItemType.None;
            if (!isEmpty)
            {
                _selectedCell = index;
            }

            SetButtonsInteractable(!isEmpty && isInventory, !isEmpty && !isInventory);
        }

        private void SetButtonsInteractable(bool toValue, bool fromValue)
        {
            _toButton.interactable = toValue;
            _toAllButton.interactable = toValue;
            _fromButton.interactable = fromValue;
            _fromAllButton.interactable = fromValue;
        }
    }
}