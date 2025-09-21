using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Island.Gameplay.Panels.HUD
{
    public class StatBarView : MonoBehaviour
    {
        [SerializeField]
        private Image _image;
        [SerializeField]
        private TMP_Text _valueTMP;
        [SerializeField]
        private Slider _slider;
        
        private int _value;
        private int _range;

        public void UpdateValue(int value)
        {
            Setup(value, _range);
        }

        public void UpdateRange(int value)
        {
            Setup(_value, value);
        }

        public void Setup(int value, int range)
        {
            _value = value;
            _range = range;

            _valueTMP.SetText($"{_value} / {_range}");
            _slider.value = (float)_value / _range;
        }

        public void SetSprite(Sprite sprite)
        {
            _image.sprite = sprite;
        }
    }
}
