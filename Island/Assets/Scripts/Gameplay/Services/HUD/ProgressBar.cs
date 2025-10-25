using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Island.Gameplay.Services.HUD
{
    public class ProgressBar : MonoBehaviour
    {
        private Tween _tween;

        [SerializeField] private Slider _slider;
        [SerializeField] private Canvas _canvas;

        private void Start()
        {
            _canvas.enabled = false;
            _canvas.worldCamera = Camera.main;
        }

        public async UniTask Show(float duration)
        {
            _slider.value = 0;
            _canvas.enabled = true;
            _tween = _slider.DOValue(1, duration).SetEase(Ease.Linear).OnUpdate(onUpdate).OnComplete(Hide);
            await _tween.AwaitForComplete();

            void onUpdate()
            {
                transform.LookAt(transform.position + _canvas.worldCamera.transform.rotation * Vector3.forward, _canvas.worldCamera.transform.rotation * Vector3.up);
            }
        }

        public void Hide()
        {
            _tween?.Kill();
            _canvas.enabled = false;
        }

        private void OnDestroy()
        {
            _tween?.Kill();
        }
    }
}