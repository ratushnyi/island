using Island.Common.Services;
using Island.Gameplay.Configs.World;
using Island.Gameplay.Profiles;
using Island.Gameplay.Services.World.Items;
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
        private WorldProfile _worldProfile;

        [Inject]
        private void Construct(NetworkService networkService, WorldConfig worldConfig, WorldProfile worldProfile)
        {
            _worldProfile = worldProfile;
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
                if (_worldProfile.WorldItemRemoved.Contains(item.Hash))
                {
                    continue;
                }
                _networkService.Spawn(item);
            }
        }

        public void MarkDestroyed(WorldItemObject worldItemObject)
        {
            _worldProfile.WorldItemRemoved.Add(worldItemObject.Hash);
        }
    }
}