using Cysharp.Threading.Tasks;
using Island.Menu.Panels.Join;
using Island.Menu.Panels.Settings;
using TendedTarsier.Core.Modules.Menu;
using TendedTarsier.Core.Panels;
using UniRx;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
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

        [Inject]
        private void Construct(PanelLoader<SettingsPanel> settingsPanel, PanelLoader<JoinPanel> joinPanel)
        {
            _settingsPanel = settingsPanel;
            _joinPanel = joinPanel;
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
            NetworkManager.Singleton.StartHost();
        }

        protected override async UniTask OnNewGameButtonClick()
        {
            await base.OnNewGameButtonClick();
            
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            Debug.Log($"Relay JoinCode: {joinCode}");

            var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            
            transport.SetRelayServerData(
                alloc.RelayServer.IpV4, (ushort)alloc.RelayServer.Port,
                alloc.AllocationIdBytes, alloc.Key, alloc.ConnectionData,
                hostConnectionDataBytes: null,
                isSecure: false
            );

            NetworkManager.Singleton.StartHost();
        }

        private async UniTask OnJoinButtonClick()
        {
            var panel = await _joinPanel.Show();
            var result = await panel.Result.Task;
            if (string.IsNullOrEmpty(result))
            {
                return;
            }
            
            await base.OnContinueButtonClick();
            JoinAllocation join = await RelayService.Instance.JoinAllocationAsync(result);
            var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            transport.SetRelayServerData(
                join.RelayServer.IpV4, (ushort)join.RelayServer.Port,
                join.AllocationIdBytes, join.Key, join.ConnectionData,
                join.HostConnectionData,
                isSecure: false);

            NetworkManager.Singleton.StartClient();
        }

        private void OnSettingsButtonClick()
        {
            _settingsPanel.Show().Forget();
        }
    }
}