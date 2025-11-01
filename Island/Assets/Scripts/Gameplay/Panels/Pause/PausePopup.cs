using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Island.Common.Services;
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
        [SerializeField] private TMP_Text _joinCode;
        [SerializeField] private List<Button> _closeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;
        private PanelLoader<SettingsPopup> _settingsPanel;
        private NetworkService _networkService;

        [Inject]
        private void Construct(PanelLoader<SettingsPopup> settingsPanel, NetworkService networkService)
        {
            _settingsPanel = settingsPanel;
            _networkService = networkService;
        }

        protected override void Initialize()
        {
            if (_networkService.IsServer)
            {
                _joinCode.SetText(_networkService.JoinCode);
            }
            else
            {
                _closeButton.ForEach(t => t.gameObject.SetActive(!_networkService.IsServerPaused.Value));;
                _joinCode.gameObject.SetActive(false);
            }
            _settingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsButtonClick()).AddTo(this);
            _exitButton.OnClickAsObservable().Subscribe(_ => HideWithResult(true)).AddTo(this);
        }

        private void OnSettingsButtonClick()
        {
            _settingsPanel.Show().Forget();
        }
    }
}