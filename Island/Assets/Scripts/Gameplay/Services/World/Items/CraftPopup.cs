using System.Collections.Generic;
using Island.Gameplay.Configs.Craft;
using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Panels.Inventory;
using TendedTarsier.Core.Panels;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Island.Gameplay.Services.World.Items
{
    public class CraftPopup : ResultPopupBase<CraftReceipt?>
    {
        [SerializeField] private Button _buttonPlus;
        [SerializeField] private Button _buttonMinus;
        [SerializeField] private Button _buttonCraft;
        [SerializeField] private TMP_InputField _countField;
        [SerializeField] private Transform _receiptsContainer;
        [SerializeField] private List<InventoryCellView> _ingredientViews;

        [Inject] private InventoryConfig _inventoryConfig;
        [Inject] private CraftConfig _craftConfig;
        private CraftReceipt _currentReceipt;

        [Inject]
        private void Construct(WorldObjectType type)
        {
            var receipts = _craftConfig.Receipts[type];
            foreach (var receipt in receipts)
            {
                var view = Instantiate(_craftConfig.CraftReceiptView, _receiptsContainer);
                view.Init(receipt, _inventoryConfig);
                view.OnButtonClicked.Subscribe(InitReceiptIngredients).AddTo(this);
            }

            _buttonCraft.OnClickAsObservable().Subscribe(_ => Craft()).AddTo(this);
        }

        private void InitReceiptIngredients(CraftReceipt receipt)
        {
            _currentReceipt = receipt;
            for (int i = 0; i < _ingredientViews.Count; i++)
            {
                if (receipt.Ingredients.Length > i)
                {
                    _ingredientViews[i].SetItem(_inventoryConfig[receipt.Ingredients[i].Type], receipt.Ingredients[i].Count);
                }
                else
                {
                    _ingredientViews[i].SetEmpty();
                }
            }
        }

        private void Craft()
        {
            HideWithResult(_currentReceipt);
        }
    }
}