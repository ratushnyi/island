using Cysharp.Threading.Tasks;
using TendedTarsier.Core.Modules.Menu;
using TendedTarsier.Core.Panels;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Island.Menu.Panels
{
    public class IslandMenuPanel : MenuPanel
    {
        [SerializeField] private Button _joinButton;
        [SerializeField] private Button _settingsButton;
        private PanelLoader<SettingsPanel.SettingsPanel> _settingsPanel;

        [Inject]
        private void Construct(PanelLoader<SettingsPanel.SettingsPanel> settingsPanel)
        {
            _settingsPanel = settingsPanel;
        }

        protected override void InitButtons()
        {
            base.InitButtons();
            RegisterButton(_settingsButton);
        }

        protected override void SubscribeButtons()
        {
            base.SubscribeButtons();
            _joinButton.OnClickAsObservable().Subscribe(_ => OnJoinButtonClick()).AddTo(CompositeDisposable);
            _settingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsButtonClick()).AddTo(CompositeDisposable);
        }
        
        protected override async UniTask OnContinueButtonClick()
        {
            await base.OnContinueButtonClick();
            NetworkManager.Singleton.StartHost();
        }

        protected override async UniTask OnNewGameButtonClick()
        {
            await base.OnNewGameButtonClick();
            NetworkManager.Singleton.StartHost();
        }

        private void OnJoinButtonClick()
        {
            base.OnContinueButtonClick();
            NetworkManager.Singleton.StartClient();
        }

        private void OnSettingsButtonClick()
        {
            _settingsPanel.Show().Forget();
        }
    }
}