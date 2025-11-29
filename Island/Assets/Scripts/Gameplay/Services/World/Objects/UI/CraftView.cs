using System;
using System.Collections.Generic;
using Island.Gameplay.Configs.Craft;
using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Panels.Player.Inventory;
using Island.Gameplay.Services.Inventory;
using TendedTarsier.Core.Utilities.Extensions;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Island.Gameplay.Services.World.Objects.UI
{
    public class CraftView : MonoBehaviour
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
        private CraftReceiptEntity _currentReceiptEntity;
        private int _count = 1;

        public IObservable<(CraftReceiptEntity Receipt, int Count)> OnCraft => _onCraft;
        private readonly ISubject<(CraftReceiptEntity Receipt, int Count)> _onCraft = new Subject<(CraftReceiptEntity Receipt, int Count)>();

        public void Initialize(WorldObjectType type)
        {
            var receipts = _craftConfig.Receipts[type];
            for (var index = 0; index < receipts.Count; index++)
            {
                var view = Instantiate(_craftConfig.WorldCraftReceiptView, _receiptsContainer);
                view.Init(receipts[index].Entity, _inventoryConfig);
                view.OnButtonClicked.Subscribe(OnReceiptClicked).AddTo(this);
                if (index != 0)
                {
                    continue;
                }

                _currentReceiptEntity = receipts[index].Entity;
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
            UpdateReceiptIngredients();
        }

        private void OnReceiptClicked(CraftReceiptEntity receiptEntity)
        {
            _currentReceiptEntity = receiptEntity;
            ChangeCount(1);
        }

        public void UpdateReceiptIngredients()
        {
            for (int i = 0; i < _ingredientViews.Count; i++)
            {
                Color color;
                if (_currentReceiptEntity.Ingredients.Length > i)
                {
                    color = _inventoryService.IsEnough(_currentReceiptEntity.Ingredients[i], _count) ? Color.green : Color.red;
                    _ingredientViews[i].SetItem(_inventoryConfig[_currentReceiptEntity.Ingredients[i].Type], _currentReceiptEntity.Ingredients[i].Count * _count);
                }
                else
                {
                    color = Color.white;
                    _ingredientViews[i].SetEmpty();
                }

                var colors = _ingredientViews[i].Button.colors;
                colors.disabledColor = color;
                _ingredientViews[i].Button.colors = colors;
            }

            UpdateCraftButtonState();
        }

        private void UpdateCraftButtonState()
        {
            var result = true;
            for (int i = 0; i < _currentReceiptEntity.Ingredients.Length; i++)
            {
                if (!_inventoryService.IsEnough(_currentReceiptEntity.Ingredients[i], _count))
                {
                    result = false;
                    break;
                }
            }

            _buttonCraft.interactable = result;
        }

        private void Craft()
        {
            _onCraft.OnNext((_currentReceiptEntity, _count));
        }
    }
}