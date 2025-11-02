using System.Collections.Generic;
using Island.Gameplay.Configs.Craft;
using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Panels.Inventory;
using Island.Gameplay.Services.Inventory;
using TendedTarsier.Core.Panels;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Island.Gameplay.Services.World.Objects.UI
{
    public class WorldCraftPopup : ResultPopupBase<(CraftReceipt Receipt, int Count)>
    {
        [SerializeField] private Button _buttonPlus;
        [SerializeField] private Button _buttonMinus;
        [SerializeField] private Button _buttonCraft;
        [SerializeField] private TMP_InputField _countField;
        [SerializeField] private Transform _receiptsContainer;
        [SerializeField] private List<InventoryCellView> _ingredientViews;

        [Inject] private InventoryService _inventoryService;
        [Inject] private InventoryConfig _inventoryConfig;
        [Inject] private CraftConfig _craftConfig;
        [Inject] private EventSystem _eventSystem;
        private CraftReceipt _currentReceipt;
        private int _count = 1;

        [Inject]
        private void Construct(WorldObjectType type)
        {
            var receipts = _craftConfig.Receipts[type];
            for (var index = 0; index < receipts.Count; index++)
            {
                var view = Instantiate(_craftConfig.WorldCraftReceiptView, _receiptsContainer);
                view.Init(receipts[index], _inventoryConfig);
                view.OnButtonClicked.Subscribe(OnReceiptClicked).AddTo(this);
                if (index != 0)
                {
                    continue;
                }
                _currentReceipt = receipts[index];
                _eventSystem.SetSelectedGameObject(view.gameObject);
            }

            ChangeCount(_count);
            _buttonCraft.OnClickAsObservable().Subscribe(_ => Craft()).AddTo(this);
            _buttonPlus.OnClickAsObservable().Subscribe(_ => ChangeCount(_count + 1)).AddTo(this);
            _buttonMinus.OnClickAsObservable().Subscribe(_ => ChangeCount(_count - 1)).AddTo(this);
            _countField.onValueChanged.AsObservable().Subscribe(OnCountFieldChanged).AddTo(this);
        }

        private void OnCountFieldChanged(string value)
        {
            if (int.TryParse(value, out var count))
            {
                ChangeCount(count);
            }
        }

        private void ChangeCount(int count)
        {
            _count = Mathf.Max(count, 0);
            _countField.SetTextWithoutNotify(_count.ToString());
            InitReceiptIngredients(_currentReceipt);
            UpdateCraftButtonState();
        }

        private void OnReceiptClicked(CraftReceipt receipt)
        {
            ChangeCount(1);
            InitReceiptIngredients(receipt);
        }

        private void InitReceiptIngredients(CraftReceipt receipt)
        {
            _currentReceipt = receipt;
            for (int i = 0; i < _ingredientViews.Count; i++)
            {
                if (receipt.Ingredients.Length > i)
                {
                    var color = _inventoryService.IsEnough(_currentReceipt.Ingredients[i], _count) ? Color.green : Color.red;
                    _ingredientViews[i].SetItem(_inventoryConfig[receipt.Ingredients[i].Type], receipt.Ingredients[i].Count * _count);
                    var colors = _ingredientViews[i].Button.colors;
                    colors.disabledColor = color;
                    _ingredientViews[i].Button.colors = colors;
                }
                else
                {
                    _ingredientViews[i].SetEmpty();
                }
            }
        }

        private void UpdateCraftButtonState()
        {
            var result = true;
            for (int i = 0; i < _currentReceipt.Ingredients.Length; i++)
            {
                if (!_inventoryService.IsEnough(_currentReceipt.Ingredients[i], _count))
                {
                    result = false;
                    break;
                }
            }

            _buttonCraft.interactable = result;
        }

        private void Craft()
        {
            HideWithResult((_currentReceipt, _count));
        }
    }
}