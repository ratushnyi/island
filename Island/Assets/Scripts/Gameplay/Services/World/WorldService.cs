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

            for (var index = 0; index < _worldConfig.WorldItemPlacement.Count; index++)
            {
                var item = _worldConfig.WorldItemPlacement[index];
                if (_worldProfile.WorldItemDestroyed.Contains(item.Hash))
                {
                    continue;
                }

                if (_worldProfile.WorldItemHealth.TryGetValue(item.Hash, out var health))
                {
                    item.Health = health;
                }

                _networkService.Spawn(item);
            }
        }

        public void MarkDestroyed(WorldItemObject worldItemObject)
        {
            _worldProfile.WorldItemDestroyed.Add(worldItemObject.Hash);
        }

        public void MarkHealth(WorldItemObject worldItemObject)
        {
            if (worldItemObject.Health.Value == 0)
            {
                _worldProfile.WorldItemHealth.Remove(worldItemObject.Hash);
            }

            if (!_worldProfile.WorldItemHealth.TryAdd(worldItemObject.Hash, worldItemObject.Health.Value))
            {
                _worldProfile.WorldItemHealth[worldItemObject.Hash] = worldItemObject.Health.Value;
            }
        }
    }
}