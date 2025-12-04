using System;
using Island.Gameplay.Services.Inventory.Items;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Island.Gameplay.Panels.Player.Inventory
{
    public class InventoryCellView : MonoBehaviour
    {
        private readonly ISubject<InventoryItemType> _onButtonClicked = new Subject<InventoryItemType>();

        [SerializeField] private Image _image;
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _countTMP;

        private ItemModel _model;

        public Button Button => _button;
        public IObservable<InventoryItemType> OnButtonClicked => _onButtonClicked;
        public InventoryItemType Type => _model?.Type ?? InventoryItemType.None;
        public bool IsEmpty() => _model == null;

        private void Start()
        {
            _button.OnClickAsObservable().Subscribe(_ => _onButtonClicked.OnNext(Type)).AddTo(this);
        }

        public void SetItem(ItemModel model, ReactiveProperty<int> count)
        {
            count.Subscribe(OnCountChanged).AddTo(this);
            SetItemModel(model);
        }

        public void SetItem(ItemModel model, int count)
        {
            OnCountChanged(count);
            SetItemModel(model);
        }

        private void SetItemModel(ItemModel model)
        {
            _model = model;
            _image.sprite = _model.Sprite;
            _image.enabled = true;
            _countTMP.enabled = model.IsCountable;
        }

        private void OnCountChanged(int count)
        {
            if (count == 0)
            {
                SetEmpty();
                return;
            }

            _countTMP.SetText(count.ToString());
        }

        public void SetEmpty()
        {
            _model = null;
            _image.sprite = null;
            _image.enabled = false;
            _countTMP.enabled = false;
        }

        public void SetSelected(bool isSelected)
        {
            var eventData = new BaseEventData(null);
            if (isSelected)
            {
                _button.OnSelect(eventData);
            }
            else
            {
                _button.OnDeselect(eventData);
            }
        }
    }
}