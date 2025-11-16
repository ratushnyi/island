using Island.Gameplay.Services.Server;
using TendedTarsier.Core.Services;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Island.Common.Services.Network
{
    public class NetworkServiceBase<T> : ServiceBase, IServerPreInitialize where T : NetworkBehaviour
    {
        [Inject] private ServerService _serverService;
        [Inject] private T _facadePrefab;
        protected T Facade;

        void IServerPreInitialize.OnNetworkPreInitialize()
        {
            if (_serverService.IsServer)
            {
                Facade = Instantiate(_facadePrefab);
                Facade.NetworkObject.Spawn();
            }
            else
            {
                Facade = Object.FindFirstObjectByType<T>();
            }
        }
    }
}