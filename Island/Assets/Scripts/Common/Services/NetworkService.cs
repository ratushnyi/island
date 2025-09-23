using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using TendedTarsier.Core.Services;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Island.Common.Services
{
    [UsedImplicitly]
    public class NetworkService : ServiceBase
    {
        public void Shutdown()
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        public async UniTask StartHost()
        {
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
        
        public async UniTask StartClient(string joinCode)
        {
            JoinAllocation join = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            transport.SetRelayServerData(
                join.RelayServer.IpV4, (ushort)join.RelayServer.Port,
                join.AllocationIdBytes, join.Key, join.ConnectionData,
                join.HostConnectionData,
                isSecure: false);

            NetworkManager.Singleton.StartClient();
        }
    }
}