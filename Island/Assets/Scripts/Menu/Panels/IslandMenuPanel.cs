using System;
using Cysharp.Threading.Tasks;
using Island.Common.Services;
using Island.Menu.Panels.Join;
using Island.Menu.Panels.Settings;
using TendedTarsier.Core.Modules.Menu;
using TendedTarsier.Core.Panels;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Island.Menu.Panels
{
    public class IslandMenuPanel : MenuPanel
    {
        [SerializeField] private Button _joinButton;
        [SerializeField] private Button _settingsButton;
        private PanelLoader<SettingsPanel> _settingsPanel;
        private PanelLoader<JoinPanel> _joinPanel;
        private NetworkService _networkService;

        [Inject]
        private void Construct(PanelLoader<SettingsPanel> settingsPanel, PanelLoader<JoinPanel> joinPanel, NetworkService networkService)
        {
            _settingsPanel = settingsPanel;
            _joinPanel = joinPanel;
            _networkService = networkService;
        }

        protected override void InitButtons()
        {
            EnsureServices().Forget();
            base.InitButtons();
            RegisterButton(_settingsButton);
        }

        private static async UniTask EnsureServices()
        {
            if (Unity.Services.Core.UnityServices.State != Unity.Services.Core.ServicesInitializationState.Initialized)
            {
                await Unity.Services.Core.UnityServices.InitializeAsync();
                if (!Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
                    await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }

        protected override void SubscribeButtons()
        {
            base.SubscribeButtons();
            _joinButton.OnClickAsObservable().Subscribe(_ => OnJoinButtonClick().Forget()).AddTo(CompositeDisposable);
            _settingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsButtonClick()).AddTo(CompositeDisposable);
        }
        
        protected override async UniTask OnContinueButtonClick()
        {
            await base.OnContinueButtonClick();
            await _networkService.StartHost();
        }

        protected override async UniTask OnNewGameButtonClick()
        {
            await base.OnNewGameButtonClick();
            await _networkService.StartHost();
        }

        private async UniTask OnJoinButtonClick()
        {
            var panel = await _joinPanel.Show();
            var joinCode = await panel.WaitForResult();
            if (string.IsNullOrEmpty(joinCode))
            {
                return;
            }

            await _networkService.TryStartClient(joinCode, base.OnContinueButtonClick);
        }

        private void OnSettingsButtonClick()
        {
            _settingsPanel.Show().Forget();
        }
    }
}