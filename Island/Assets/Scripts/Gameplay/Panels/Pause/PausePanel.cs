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
    public class PausePanel : ResultPanelBase<bool>
    {
        [SerializeField] private TMP_Text _joinCode;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;
        private PanelLoader<SettingsPanel> _settingsPanel;
        private NetworkService _networkService;

        [Inject]
        private void Construct(PanelLoader<SettingsPanel> settingsPanel, NetworkService networkService)
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
                _closeButton.gameObject.SetActive(!_networkService.IsServerPaused.Value);
                _joinCode.gameObject.SetActive(false);
            }
            _settingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsButtonClick()).AddTo(CompositeDisposable);
            _exitButton.OnClickAsObservable().Subscribe(_ => HideWithResult(true)).AddTo(CompositeDisposable);
        }

        private void OnSettingsButtonClick()
        {
            _settingsPanel.Show().Forget();
        }
    }
}