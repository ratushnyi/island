using System;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
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
        private NetworkConfig _config;
        private NetworkServiceFacade _networkServiceFacade;

        public bool IsReady => NetworkManager.Singleton.IsApproved;
        public bool IsServer => NetworkManager.Singleton.IsServer;
        public bool IsClient => NetworkManager.Singleton.IsClient;
        public bool IsHost => NetworkManager.Singleton.IsHost;
        public IObservable<Unit> OnServerStopped => Observable.FromEvent(t => NetworkManager.Singleton.OnPreShutdown += t, t => NetworkManager.Singleton.OnPreShutdown -= t);
        public void SetPaused(bool value) => _networkServiceFacade.SetPaused(value);
        public IReadOnlyReactiveProperty<bool> IsServerPaused => _networkServiceFacade.IsPaused;

        [Inject]
        private void Construct(NetworkConfig config)
        {
            _config = config;
        }

        public void Initialize(NetworkServiceFacade networkServiceFacade)
        {
            _networkServiceFacade = networkServiceFacade;
        }

        public void Shutdown()
        {
            NetworkManager.Singleton.Shutdown();
        }

        public async UniTask StartHost()
        {
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(3);
            GUIUtility.systemCopyBuffer = JoinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            Debug.Log($"Relay JoinCode: {JoinCode}");

            var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

            transport.SetRelayServerData(
                alloc.RelayServer.IpV4, (ushort)alloc.RelayServer.Port,
                alloc.AllocationIdBytes, alloc.Key, alloc.ConnectionData,
                hostConnectionDataBytes: null,
                isSecure: false
            );

            NetworkManager.Singleton.StartHost();
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