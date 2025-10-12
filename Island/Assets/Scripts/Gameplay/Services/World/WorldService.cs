using Island.Common.Services;
using Island.Common.Services.Network;
using Island.Gameplay.Configs.World;
using JetBrains.Annotations;
using TendedTarsier.Core.Services;
using Zenject;

namespace Island.Gameplay.Services.World
{
    [UsedImplicitly]
    public class WorldService : ServiceBase, INetworkInitialize
    {
        private NetworkService _networkService;
        private WorldConfig _worldConfig;

        [Inject]
        private void Construct(NetworkService networkService, WorldConfig worldConfig)
        {
            _worldConfig = worldConfig;
            _networkService = networkService;
        }
        
        public void OnNetworkInitialize()
        {
            if (!_networkService.IsServer)
            {
                return;
            }
            
            foreach (var item in _worldConfig.WorldItemPlacement)
            {
                _networkService.Spawn(new NetworkSpawnRequest(item.Type, item.transform.position, item.transform.rotation));
            }
        }
    }
}