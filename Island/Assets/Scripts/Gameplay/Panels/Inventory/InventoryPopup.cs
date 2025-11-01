using System.Linq;
using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Profiles.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using TendedTarsier.Core.Panels;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Island.Gameplay.Panels.Inventory
{
    public class InventoryPopup : PopupBase
    {
        [SerializeField] private Transform _gridContainer;
        [SerializeField] private InventoryCellView _freehandCell;
        [Inject] private InventoryProfile _inventoryProfile;
        [Inject] private InventoryConfig _inventoryConfig;
        [Inject] private EventSystem _eventSystem;
        private InventoryCellView[] _cellsList;

        protected override void Initialize()
        {
            _inventoryProfile.InventoryItems.ObserveAdd().Subscribe(Put).AddTo(this);
            _cellsList = new InventoryCellView[_inventoryConfig.InventoryCapacity + 1];
            _cellsList[0] = _freehandCell;
            _cellsList[0].SetItem(_inventoryConfig[InventoryItemType.Hand], _inventoryProfile.InventoryItems[InventoryItemType.Hand]);
            _cellsList[0].OnButtonClicked.Subscribe(onCellClicked).AddTo(this);
            _eventSystem.SetSelectedGameObject(_cellsList[0].gameObject);

            for (var i = 1; i < _cellsList.Length; i++)
            {
                _cellsList[i] = Instantiate(_inventoryConfig.InventoryCellView, _gridContainer);
                _cellsList[i].OnButtonClicked.Subscribe(onCellClicked).AddTo(this);

                if (_inventoryProfile.InventoryItems.Count > i)
                {
                    var item = _inventoryProfile.InventoryItems.ElementAt(i);
                    SetItem(_cellsList[i], item.Key, item.Value);
                    if (_inventoryProfile.SelectedItem.Value == item.Key)
                    {
                        _eventSystem.SetSelectedGameObject(_cellsList[i].gameObject);
                    }
                }
            }

            void onCellClicked(InventoryItemType type)
            {
                if (type == InventoryItemType.None)
                {
                    return;
                }

                _inventoryProfile.SelectedItem.Value = type;
                _inventoryProfile.Save();
            }
        }

        private void Put(DictionaryAddEvent<InventoryItemType, ReactiveProperty<int>> item)
        {
            var cell = _cellsList.FirstOrDefault(t => t.IsEmpty());
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