using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Island.Gameplay.Services.HUD
{
    public class ProgressBar : MonoBehaviour, IDisposable
    {
        private Tween _tween;

        [SerializeField]
        private Slider _slider;
        [SerializeField]
        private Canvas _canvas;

        private void Start()
        {
            _canvas.enabled = false;
            _canvas.worldCamera = Camera.main;
        }

        public async UniTask Show(float duration)
        {
            _slider.value = 0;
            _canvas.enabled = true;
            _tween = _slider.DOValue(1, duration).SetEase(Ease.Linear).OnUpdate(OnUpdate).OnComplete(Dispose);
            await _tween.AwaitForComplete();
            Dispose();
        }

        private void OnUpdate()
        {
            transform.LookAt(transform.position + _canvas.worldCamera.transform.rotation * Vector3.forward, _canvas.worldCamera.transform.rotation * Vector3.up);
        }

        public void Dispose()
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