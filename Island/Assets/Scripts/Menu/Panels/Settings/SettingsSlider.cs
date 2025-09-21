using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Island.Menu.Panels.Setting
{
    public class SettingsSlider : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleTMP;
        [SerializeField] private TMP_Text _valueTMP;
        [SerializeField] private Slider _slider;
        private Vector2Int _range;
        private readonly ISubject<int> _onValueChangedSubject = new Subject<int>();
        public IObservable<int> OnValueChangedObservable => _onValueChangedSubject;
        public int Value { get; private set; }

        public void Init(string title, int value, Vector2Int range)
        {
            Value = value;
            _range = range;
            _titleTMP.SetText(title);
            _valueTMP.SetText(value.ToString());
            _slider.SetValueWithoutNotify(Mathf.InverseLerp(range.x, range.y, value));
            _slider.OnValueChangedAsObservable().Subscribe(OnSlider).AddTo(this);
        }

        private void OnSlider(float value)
        {
            Value = (int)Mathf.Lerp(_range.x, _range.y, value);
            _valueTMP.SetText(Value.ToString());
            _onValueChangedSubject.OnNext(Value);
        }
    }
}