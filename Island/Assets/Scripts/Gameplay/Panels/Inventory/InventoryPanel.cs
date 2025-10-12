using System.Linq;
using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Profiles.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using TendedTarsier.Core.Panels;
using UniRx;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Panels.Inventory
{
    public class InventoryPanel : PopupBase
    {
        [SerializeField]
        private Transform _gridContainer;
        private InventoryProfile _inventoryProfile;
        private InventoryConfig _inventoryConfig;
        private InventoryCellView[] _cellsList;

        public InventoryCellView FirstCellView => _cellsList?[0];

        [Inject]
        private void Construct(
            InventoryProfile inventoryProfile,
            InventoryConfig inventoryConfig)
        {
            _inventoryProfile = inventoryProfile;
            _inventoryConfig = inventoryConfig;

            _inventoryProfile.InventoryItems.ObserveAdd().Subscribe(Put).AddTo(this);
        }

        protected override void Initialize()
        {
            _cellsList = new InventoryCellView[_inventoryConfig.InventoryCapacity];
            for (var i = 0; i < _inventoryConfig.InventoryCapacity; i++)
            {
                _cellsList[i] = Instantiate(_inventoryConfig.InventoryCellView, _gridContainer);
                _cellsList[i].OnButtonClicked.Subscribe(onCellClicked).AddTo(this);

                if (_inventoryProfile.InventoryItems.Count > i)
                {
                    var item = _inventoryProfile.InventoryItems.ElementAt(i);
                    SetItem(_cellsList[i], item.Key, item.Value);
                }
            }

            void onCellClicked(InventoryItemType type)
            {
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