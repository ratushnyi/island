using System.Linq;
using Island.Common;
using Island.Common.Services;
using Island.Common.Services.Network;
using Island.Gameplay.Configs.World;
using Island.Gameplay.Profiles;
using Island.Gameplay.Services.Inventory.Items;
using Island.Gameplay.Services.World.Items;
using JetBrains.Annotations;
using TendedTarsier.Core.Services;
using UnityEngine;
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

            for (var index = 0; index < _worldConfig.WorldObjectPlacement.Count; index++)
            {
                var request = _worldConfig.WorldObjectPlacement[index];
                if (_worldProfile.DestroyedObject.Contains(request.Hash))
                {
                    continue;
                }

                if (_worldProfile.ObjectHealthDictionary.TryGetValue(request.Hash, out var health))
                {
                    request.Health = health;
                }

                if (_worldProfile.ObjectContainerDictionary.TryGetValue(request.Hash, out var container))
                {
                    request.Container = container.ToItemsList();
                }

                _networkService.Spawn(request);
            }
            
            for (var index = 0; index < _worldProfile.SpawnedObjects.Values.Count; index++)
            {
                var request = _worldProfile.SpawnedObjects.Values.ElementAt(index);
                if (_worldProfile.ObjectHealthDictionary.TryGetValue(request.Hash, out var health))
                {
                    request.Health = health;
                }

                if (_worldProfile.ObjectContainerDictionary.TryGetValue(request.Hash, out var container))
                {
                    request.Container = container.ToItemsList();
                }

                _networkService.Spawn(request);
            }
        }

        public void SpawnResultItem(WorldObjectBase worldItemObject)
        {
            var type = WorldObjectType.Collectable;
            var position = worldItemObject.transform.position + Vector3.up + Vector3.up;
            var hash = IslandExtensions.GenerateHash(type, position);
            var request = new NetworkSpawnRequest(hash, type, position, resultItem: worldItemObject.ResultItem);
            _worldProfile.SpawnedObjects.Add(hash, request);
            _networkService.Spawn(request);
        }

        public void MarkDestroyed(WorldObjectBase worldItemObject)
        {
            _worldProfile.SpawnedObjects.Remove(worldItemObject.Hash);
            _worldProfile.DestroyedObject.Add(worldItemObject.Hash);
        }

        public void UpdateHealth(WorldObjectBase worldItemObject)
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

        public void UpdateContainer(WorldObjectBase worldItemObject, InventoryItemType type, int count)
        {
            if (!_worldProfile.ObjectContainerDictionary.TryGetValue(worldItemObject.Hash, out var container))
            {
                if (count == 0)
                {
                    return;
                }
                container = new ObjectContainer();
                _worldProfile.ObjectContainerDictionary.Add(worldItemObject.Hash, container);
            }
            
            if (count == 0)
            {
                container.Items.Remove(type);
            }
            else
            {
                container.Items[type] = count;
            }

            if (container.Items.Count == 0)
            {
                _worldProfile.ObjectContainerDictionary.Remove(worldItemObject.Hash);
            }
        }
    }
}