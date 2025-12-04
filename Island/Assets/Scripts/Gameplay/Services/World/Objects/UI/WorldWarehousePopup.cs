using System;
using System.Linq;
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
        [Inject] private InventoryService _inventoryService;
        [Inject] private EventSystem _eventSystem;
        private InventoryCellView[] _inventoryCellsList;
        private InventoryCellView[] _warehouseCellsList;
        private InventoryItemType _selectedItem;
        private WorldContainerBase _container;
        private readonly ReactiveDictionary<InventoryItemType, ReactiveProperty<int>> _dictionary = new();
        private InventoryCellView _selectedInventoryCell;
        private InventoryCellView _selectedWarehouseCell;

        [Inject]
        private void Initialize(WorldContainerBase container)
        {
            _container = container;
            InitializeContainerDictionary();
            SetButtonsInteractable(false, false);
            _toButton.OnClickAsObservable().Subscribe(_ => OnMoveButtonClicked(true, false)).AddTo(this);
            _toAllButton.OnClickAsObservable().Subscribe(_ => OnMoveButtonClicked(true, true)).AddTo(this);
            _fromButton.OnClickAsObservable().Subscribe(_ => OnMoveButtonClicked(false, false)).AddTo(this);
            _fromAllButton.OnClickAsObservable().Subscribe(_ => OnMoveButtonClicked(false, true)).AddTo(this);
            _inventoryProfile.InventoryItems.ObserveAdd().Subscribe(PutToInventory).AddTo(this);
            InitGrid(_inventoryGridContainer, _inventoryConfig.InventoryCapacity, _inventoryProfile.InventoryItems, out _inventoryCellsList);
            InitGrid(_warehouseGridContainer, _capacity, _dictionary, out _warehouseCellsList);
        }

        private void InitializeContainerDictionary()
        {
            _dictionary.Clear();
            foreach (var item in _container.ContainerArray)
            {
                _dictionary.Add(item.Type, new ReactiveProperty<int>(item.Count));
            }

            _container.OnContainerChanged.Subscribe(onChanged).AddTo(this);

            void onChanged(NetworkListEvent<ItemEntity> entity)
            {
                switch (entity.Type)
                {
                    case NetworkListEvent<ItemEntity>.EventType.Add:
                        _dictionary.Add(entity.Value.Type, new ReactiveProperty<int>(entity.Value.Count));
                        SetItem(_warehouseCellsList.First(t => t.IsEmpty()), entity.Value.Type, _dictionary[entity.Value.Type]);
                        break;
                    case NetworkListEvent<ItemEntity>.EventType.RemoveAt:
                        _dictionary.Remove(entity.Value.Type);
                        _warehouseCellsList.First(t => t.Type == entity.Value.Type).SetEmpty();
                        break;
                    case NetworkListEvent<ItemEntity>.EventType.Value:
                        if (_dictionary.TryGetValue(entity.Value.Type, out var value))
                        {
                            value.Value = entity.Value.Count;
                        }

                        break;
                }
            }
        }

        private void OnMoveButtonClicked(bool isTo, bool isAll)
        {
            var count = 1;
            if (isTo)
            {
                if (!_inventoryProfile.InventoryItems.TryGetValue(_selectedItem, out var item))
                {
                    return;
                }

                if (isAll)
                {
                    count = item.Value;
                }

                _inventoryService.TryRemove(_selectedItem, count);
            }
            else
            {
                if (!_container.ContainerArray.TryGet(t => t.Type == _selectedItem, out var item))
                {
                    return;
                }

                if (isAll)
                {
                    count = item.Count;
                }

                _inventoryService.TryCollect(_selectedItem, count);
            }

            _container.ApplyContainer_ServerRpc(new ItemEntity[] { new(_selectedItem, isTo ? count : -count) });
            OnCellClicked(_selectedItem);
        }

        private void InitGrid(Transform container, int capacity, ReactiveDictionary<InventoryItemType, ReactiveProperty<int>> items, out InventoryCellView[] cellList)
        {
            cellList = new InventoryCellView[capacity];

            var validItems = items.Where(t => t.Key > 0).ToList();
            for (var i = 0; i < capacity; i++)
            {
                cellList[i] = Instantiate(_inventoryConfig.InventoryCellView, container);
                cellList[i].OnButtonClicked.Subscribe(OnCellClicked).AddTo(this);

                if (validItems.Count > i)
                {
                    var item = validItems.ElementAt(i);
                    SetItem(cellList[i], item.Key, item.Value);
                }
            }
        }

        private void OnCellClicked(InventoryItemType type)
        {
            if (type == InventoryItemType.None)
            {
                return;
            }

            _selectedItem = type;

            _eventSystem.SetSelectedGameObject(null);
            var inventoryCell = _inventoryCellsList.FirstOrDefault(t => t.Type == type);
            if (inventoryCell != _selectedInventoryCell)
            {
                _selectedInventoryCell?.SetSelected(false);
                _selectedInventoryCell = inventoryCell;
                _selectedInventoryCell?.SetSelected(true);
            }

            var warehouseCell = _warehouseCellsList.FirstOrDefault(t => t.Type == type);
            if (warehouseCell != _selectedWarehouseCell)
            {
                _selectedWarehouseCell?.SetSelected(false);
                _selectedWarehouseCell = warehouseCell;
                _selectedWarehouseCell?.SetSelected(true);
            }

            SetButtonsInteractable(inventoryCell != null, warehouseCell != null);
        }

        private void SetButtonsInteractable(bool toValue, bool fromValue)
        {
            _toButton.interactable = toValue;
            _toAllButton.interactable = toValue;
            _fromButton.interactable = fromValue;
            _fromAllButton.interactable = fromValue;
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