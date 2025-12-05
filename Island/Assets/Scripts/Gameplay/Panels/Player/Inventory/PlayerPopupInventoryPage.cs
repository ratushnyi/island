using System.Linq;
using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Profiles.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Island.Gameplay.Panels.Player.Inventory
{
    public class PlayerPopupInventoryPage : PlayerPopupPage
    {
        public override string Name => "Inventory";

        [SerializeField] private Transform _gridContainer;
        [SerializeField] private InventoryCellView _freehandCell;
        [Inject] private InventoryProfile _inventoryProfile;
        [Inject] private InventoryConfig _inventoryConfig;
        [Inject] private EventSystem _eventSystem;
        private InventoryCellView[] _cellsList;

        public override void Initialize()
        {
            _cellsList = new InventoryCellView[_inventoryProfile.InventoryItems.Count];
            _cellsList[0] = _freehandCell;
            _cellsList[0].SetItem(_inventoryProfile.InventoryItems, _inventoryConfig[InventoryItemType.Hand], 0);
            _cellsList[0].OnButtonClicked.Subscribe(onCellClicked).AddTo(this);
            _eventSystem.SetSelectedGameObject(_cellsList[0].gameObject);

            for (var i = 1; i < _cellsList.Length; i++)
            {
                _cellsList[i] = Instantiate(_inventoryConfig.InventoryCellView, _gridContainer);
                initCell(i);
            }

            _inventoryProfile.InventoryItems.ObserveAdd().Subscribe(t => initCell(t.Index)).AddTo(this);

            void initCell(int index)
            {
                _cellsList[index].OnButtonClicked.Subscribe(onCellClicked).AddTo(this);

                var item = _inventoryProfile.InventoryItems[index];
                _cellsList[index].SetItem(_inventoryProfile.InventoryItems, _inventoryConfig[item.Type], index);
                if (_inventoryProfile.SelectedItem.Value == index)
                {
                    _eventSystem.SetSelectedGameObject(_cellsList[index].gameObject);
                }
            }

            void onCellClicked(int index)
            {
                if (_inventoryProfile.InventoryItems[index].Type == InventoryItemType.None)
                {
                    return;
                }

                _inventoryProfile.SelectedItem.Value = index;
                _inventoryProfile.Save();
            }
        }
    }
}