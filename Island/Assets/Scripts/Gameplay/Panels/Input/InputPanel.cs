using Island.Gameplay.Services.Stats;
using TendedTarsier.Core.Panels;
using TendedTarsier.Core.Services.Input;
using TendedTarsier.Core.Utilities.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Island.Gameplay.Panels.HUD
{
    public class InputPanel : PanelBase
    {
        public override bool ShowInstantly => Application.isMobilePlatform;

        [SerializeField] private Image _runImage;
        [SerializeField] private Color _runImageEnabledColor;
        [Inject] private StatsService _statService;
        [Inject] private InputService _inputService;
        private Color _runImageDefaultColor;

        protected override void Initialize()
        {
            if (!InputExtensions.IsMouseKeyboardInput)
            {
                _runImageDefaultColor = _runImage.color;
                _statService.IsSprintPerformed.Subscribe(OnSprintButtonToggleChanged).AddTo(this);
            }
        }

        private void OnSprintButtonToggleChanged(bool value)
        {
            _runImage.color = value ? _runImageEnabledColor : _runImageDefaultColor;
        }
    }
}