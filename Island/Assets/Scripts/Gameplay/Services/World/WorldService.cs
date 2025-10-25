using Island.Common.Services;
using Island.Gameplay.Configs.World;
using Island.Gameplay.Profiles;
using Island.Gameplay.Services.Inventory.Items;
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
                if (_worldProfile.WorldObjectDestroyed.Contains(item.Hash))
                {
                    continue;
                }

                if (_worldProfile.ObjectHealthDictionary.TryGetValue(item.Hash, out var health))
                {
                    item.Health = health;
                }

                _networkService.Spawn(item);
            }
        }

        public void MarkDestroyed(WorldObjectBase worldItemObject)
        {
            _worldProfile.WorldObjectDestroyed.Add(worldItemObject.Hash);
        }

        public void MarkHealth(WorldObjectBase worldItemObject)
        {
            if (worldItemObject.Health.Value == 0)
            {
                _worldProfile.ObjectHealthDictionary.Remove(worldItemObject.Hash);
            }

            if (!_worldProfile.ObjectHealthDictionary.TryAdd(worldItemObject.Hash, worldItemObject.Health.Value))
            {
                _worldProfile.ObjectHealthDictionary[worldItemObject.Hash] = worldItemObject.Health.Value;
            }
        }

        public void ObjectAdded(WorldObjectBase worldItemObject, InventoryItemType type, int count)
        {
            if(!_worldProfile.ObjectContainerDictionary.TryGetValue(worldItemObject.Hash, out var container))
            {
                container = new ObjectContainer();
                _worldProfile.ObjectContainerDictionary.Add(worldItemObject.Hash, container);
            }
            
            container.Items[type] += count;
        }
    }
}