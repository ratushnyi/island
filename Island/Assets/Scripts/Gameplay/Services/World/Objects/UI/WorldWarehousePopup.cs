using System;
using System.Linq;
using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Panels.Player.Inventory;
using Island.Gameplay.Profiles.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using TendedTarsier.Core.Panels;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Island.Gameplay.Services.World.Objects.UI
{
    public class WorldWarehousePopup : ResultPopupBase<(ItemEntity[] input, ItemEntity[] output)>
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
        [Inject] private EventSystem _eventSystem;
        private InventoryCellView[] _inventoryCellsList;
        private InventoryCellView[] _warehouseCellsList;
        private InventoryItemType _selectedItem;

        protected override void Initialize()
        {
            SetButtonsInteractable(false);
            _toButton.OnClickAsObservable().Subscribe(_ => OnMoveButtonClicked(true, false)).AddTo(this);
            _toAllButton.OnClickAsObservable().Subscribe(_ => OnMoveButtonClicked(true, true)).AddTo(this);
            _fromButton.OnClickAsObservable().Subscribe(_ => OnMoveButtonClicked(false, false)).AddTo(this);
            _fromAllButton.OnClickAsObservable().Subscribe(_ => OnMoveButtonClicked(false, true)).AddTo(this);
            _inventoryProfile.InventoryItems.ObserveAdd().Subscribe(PutToInventory).AddTo(this);
            InitGrid(_inventoryGridContainer, _inventoryConfig.InventoryCapacity, _inventoryProfile.InventoryItems, out _inventoryCellsList);
            InitGrid(_warehouseGridContainer, _capacity, _inventoryProfile.InventoryItems, out _warehouseCellsList);
            SetDefaultResult((Array.Empty<ItemEntity>(), Array.Empty<ItemEntity>()));
        }

        private void OnMoveButtonClicked(bool isTo, bool isAll)
        {
            
        }

        private void InitGrid(Transform container, int capacity, ReactiveDictionary<InventoryItemType, ReactiveProperty<int>> items, out InventoryCellView[] cellList)
        {
            cellList = new InventoryCellView[capacity];

            for (var i = 1; i < capacity; i++)
            {
                cellList[i] = Instantiate(_inventoryConfig.InventoryCellView, container);
                cellList[i].OnButtonClicked.Subscribe(OnCellClicked).AddTo(this);

                if (items.Count > i)
                {
                    var item = items.ElementAt(i);
                    SetItem(cellList[i], item.Key, item.Value);
                }
            }
        }

        private void OnCellClicked(InventoryItemType type)
        {
            SetButtonsInteractable(type != InventoryItemType.None);

            if (type == InventoryItemType.None)
            {
                return;
            }

            _selectedItem = type;
        }

        private void SetButtonsInteractable(bool value)
        {
            _toButton.interactable = value;
            _toAllButton.interactable = value;
            _fromButton.interactable = value;
            _fromAllButton.interactable = value;
        }

        private void PutToInventory(DictionaryAddEvent<InventoryItemType, ReactiveProperty<int>> item)
        {
            var cell = _inventoryCellsList.FirstOrDefault(t => t.IsEmpty());
            if (cell != null)
            {
                SetItem(cell, item.Key, item.Value);
            }
        }

        private void SetItem(InventoryCellView cell, InventoryItemType type, ReactiveProperty<int> value)
        {
            cell.SetItem(_inventoryConfig[type], value);
        }
    }
}