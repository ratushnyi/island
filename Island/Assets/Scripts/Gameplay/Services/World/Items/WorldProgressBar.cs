using UnityEngine;
using UnityEngine.UI;

namespace Island.Gameplay.Services.World.Items
{
    public class WorldProgressBar : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private Canvas _canvas;

        private void Start()
        {
            _canvas.enabled = false;
            _canvas.worldCamera = Camera.main;
        }

        public void SetValue(float value)
        {
            if (value < 0 && _canvas.enabled)
            {
                _canvas.enabled = false;
            }
            else if(!_canvas.enabled)
            {
                _canvas.enabled = true;
            }
            
            _slider.value = value;
            transform.LookAt(transform.position + _canvas.worldCamera.transform.rotation * Vector3.forward, _canvas.worldCamera.transform.rotation * Vector3.up);
        }
    }
}