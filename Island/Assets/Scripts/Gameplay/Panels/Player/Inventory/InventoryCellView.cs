using System;
using Island.Gameplay.Profiles.Inventory;
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
        private readonly ISubject<int> _onButtonClicked = new Subject<int>();

        [SerializeField] private Image _image;
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _countTMP;

        private ItemModel _model;
        private ItemStack _stack;
        private int _cellIndex;

        public Button Button => _button;
        public IObservable<int> OnButtonClicked => _onButtonClicked;
        public ItemStack Stack => _stack;
        public bool IsEmpty() => _model == null;

        private void Start()
        {
            _button.OnClickAsObservable().Subscribe(_ => _onButtonClicked.OnNext(_cellIndex)).AddTo(this);
        }

        public void SetItem(ReactiveCollection<ItemStack> collection, ItemModel model, int cellIndex)
        {
            _cellIndex = cellIndex;
            _stack = collection[_cellIndex];
            
            collection
                .ObserveReplace()
                .Where(t => t.Index == _cellIndex)
                .Subscribe(t => SetCount(t.NewValue.Count))
                .AddTo(this);

            SetCount(_stack.Count);
            SetItemModel(model);
        }

        public void SetItem(ItemModel model, int count)
        {
            SetCount(count);
            SetItemModel(model);
        }
        
        private void SetItemModel(ItemModel model)
        {
            _model = model;
            if (_model == null)
            {
                SetEmpty();
            }
            else
            {
                SetImage();
            }
        }

        private void SetImage()
        {
            _image.sprite = _model.Sprite;
            _image.enabled = true;
        }

        public void SetEmpty()
        {
            _model = null;
            _image.sprite = null;
            _image.enabled = false;
            _countTMP.enabled = false;
        }

        private void SetCount(int count)
        {
            switch (count)
            {
                case 0:
                    SetEmpty();
                    break;
                case 1:
                    _countTMP.enabled = false;
                    break;
                default:
                    _countTMP.enabled = true;
                    _countTMP.SetText(count.ToString());
                    break;
            }
        }
    }
}