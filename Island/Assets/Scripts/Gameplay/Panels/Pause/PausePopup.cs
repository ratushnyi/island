using Cysharp.Threading.Tasks;
using Island.Common.Services;
using Island.Gameplay.Services.Server;
using Island.Menu.Panels.Settings;
using TendedTarsier.Core.Panels;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Island.Gameplay.Panels.Pause
{
    public class PausePopup : ResultPopupBase<bool>
    {
        [SerializeField] private TMP_Text _serverId;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;
        [Inject] private PanelLoader<SettingsPopup> _settingsPanel;
        [Inject] private ServerService _serverService;

        protected override void Initialize()
        {
            _serverId.SetText(_serverService.ServerId);

            if (!_serverService.IsServer)
            {
                UpdateActiveCloseButton();
            }

            _settingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsButtonClick()).AddTo(this);
            _exitButton.OnClickAsObservable().Subscribe(_ => HideWithResult(true)).AddTo(this);
        }

        public void UpdateActiveCloseButton()
        {
            _closeButton.gameObject.SetActive(!_serverService.IsServerPaused.Value);
        }

        private void OnSettingsButtonClick()
        {
            _settingsPanel.Show().Forget();
        }

        public override void Hide(bool immediate = false)
        {
            if (!_serverService.IsServer && _serverService.IsServerPaused.Value)
            {
                return;
            }

            base.Hide(immediate);
        }
    }
}