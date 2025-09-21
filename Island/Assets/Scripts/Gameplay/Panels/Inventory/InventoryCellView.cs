using System;
using Island.Gameplay.Services.Inventory.Items;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Island.Gameplay.Panels.Inventory
{
    public class InventoryCellView : MonoBehaviour
    {
        private readonly CompositeDisposable _compositeDisposable = new();
        private readonly ISubject<string> _onButtonClicked = new Subject<string>();

        [SerializeField]
        private Image _image;
        [SerializeField]
        private Button _button;
        [SerializeField]
        private TextMeshProUGUI _countTMP;

        private ItemModel _model;

        public IObservable<string> OnButtonClicked => _onButtonClicked;

        private void Start()
        {
            _button.OnClickAsObservable().Subscribe(_ => _onButtonClicked.OnNext(_model?.Id)).AddTo(_compositeDisposable);
        }

        public void SetItem(ItemModel model, ReactiveProperty<int> count)
        {
            count.Subscribe(OnCountChanged).AddTo(_compositeDisposable);

            _model = model;
            _image.sprite = _model.Sprite;
            _image.enabled = true;
            _countTMP.enabled = model.IsCountable;
        }

        public bool IsEmpty()
        {
            return _model == null;
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

        private void OnDestroy()
        {
            _compositeDisposable.Clear();
        }
    }
}