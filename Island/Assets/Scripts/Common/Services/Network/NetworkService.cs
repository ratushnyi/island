using System;
using Cysharp.Threading.Tasks;
using Island.Common.Services.Network;
using Island.Gameplay.Services.Inventory;
using JetBrains.Annotations;
using TendedTarsier.Core.Modules.Project;
using TendedTarsier.Core.Services;
using UniRx;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Zenject;

namespace Island.Common.Services
{
    [UsedImplicitly]
    public class NetworkService : ServiceBase
    {
        public string JoinCode { get; private set; }
        
        [Inject] private ProjectProfile _projectProfile;
        [Inject] private NetworkConfig _config;
        private NetworkServiceFacade _networkServiceFacade;

        public bool IsReady => NetworkManager.Singleton.IsApproved;
        public bool IsServer => NetworkManager.Singleton.IsServer;
        public bool IsClient => NetworkManager.Singleton.IsClient;
        public bool IsHost => NetworkManager.Singleton.IsHost;
        public IObservable<Unit> OnShutdown => Observable.FromEvent(t => NetworkManager.Singleton.OnPreShutdown += t, t => NetworkManager.Singleton.OnPreShutdown -= t).Merge(_networkServiceFacade.OnShutdown);
        public IReadOnlyReactiveProperty<bool> IsServerPaused => _networkServiceFacade.IsPaused;
        public string ServerId => _networkServiceFacade.ServerId.Value;

        public void SetPaused(bool value) => _networkServiceFacade.SetPaused(value);
        public void Spawn(NetworkSpawnRequest request) => _networkServiceFacade.Spawn_ServerRpc(request);

        public void Initialize(NetworkServiceFacade networkServiceFacade)
        {
            _networkServiceFacade = networkServiceFacade;
        }

        public void Shutdown()
        {
            _networkServiceFacade.Shutdown_ClientRpc();
            NetworkManager.Singleton.Shutdown();
        }
        
        public async UniTask EnsureServices()
        {
            if (Unity.Services.Core.UnityServices.State != Unity.Services.Core.ServicesInitializationState.Initialized)
            {
                await Unity.Services.Core.UnityServices.InitializeAsync();
                if (!Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
                {
                    await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
            }
        }

        public async UniTask StartHost(bool isNewGame)
        {
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(3);
            GUIUtility.systemCopyBuffer = JoinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            Debug.Log($"Relay JoinCode: {JoinCode}");
            if (isNewGame)
            {
                _projectProfile.ServerId = JoinCode;
                _projectProfile.Save();
            }

            var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

            transport.SetRelayServerData(
                alloc.RelayServer.IpV4, (ushort)alloc.RelayServer.Port,
                alloc.AllocationIdBytes, alloc.Key, alloc.ConnectionData,
                hostConnectionDataBytes: null,
                isSecure: false
            );

            NetworkManager.Singleton.StartHost();

            _networkServiceFacade.SetServerId(_projectProfile.ServerId);
        }

        public async UniTask<bool> TryStartClient(string joinCode, Func<UniTask> beforeClientStarted)
        {
            JoinCode = null;

            try
            {
                JoinAllocation join = await RelayService.Instance.JoinAllocationAsync(joinCode);
                var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
                transport.SetRelayServerData(
                    join.RelayServer.IpV4, (ushort)join.RelayServer.Port,
                    join.AllocationIdBytes, join.Key, join.ConnectionData,
                    join.HostConnectionData,
                    isSecure: false);

                if (beforeClientStarted != null)
                {
                    await beforeClientStarted.Invoke();
                }

                NetworkManager.Singleton.StartClient();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}