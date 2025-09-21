using Cysharp.Threading.Tasks;
using Island.Common.Services;
using Island.Menu.Panels.Setting;
using TendedTarsier.Core.Panels;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Island.Menu.Panels.Settings
{
    public class SettingsPanel : PanelBase
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private SettingsSlider _fovSlider;
        [SerializeField] private SettingsSlider _cameraSensitivitySlider;
        private SettingsService _settingsService;

        [Inject]
        private void Construct(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        protected override void Initialize()
        {
            _fovSlider.Init("Field of view", _settingsService.Fov, new Vector2Int(50, 100));
            _cameraSensitivitySlider.Init("Camera Sensitivity", _settingsService.CameraSensitivity, new Vector2Int(1, 200));
            
            _fovSlider.OnValueChangedObservable.Subscribe(value => _settingsService.Fov = value).AddTo(CompositeDisposable);
            _cameraSensitivitySlider.OnValueChangedObservable.Subscribe(value => _settingsService.CameraSensitivity = value).AddTo(CompositeDisposable);
        }
        
        public override async UniTask ShowAnimation()
        {
            await base.ShowAnimation();
            
            _closeButton.OnClickAsObservable().Subscribe(t => PerformHide()).AddTo(CompositeDisposable);
        }
    }
}