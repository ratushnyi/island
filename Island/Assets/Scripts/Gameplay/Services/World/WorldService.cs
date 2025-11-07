using System.Linq;
using Island.Common;
using Island.Common.Services;
using Island.Common.Services.Network;
using Island.Gameplay.Configs.World;
using Island.Gameplay.Profiles;
using Island.Gameplay.Services.Inventory.Items;
using Island.Gameplay.Services.World.Objects;
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
                    request.Container = container.AsArray();
                }

                _networkService.Spawn(request, false);
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
                    request.Container = container.AsArray();
                }

                _networkService.Spawn(request, false);
            }
        }

        public void Spawn(Vector3 position, WorldObjectType type, ItemEntity collectableItem = default)
        {
            var hash = IslandExtensions.GenerateHash(position);
            while (_worldProfile.SpawnedObjects.ContainsKey(hash) || IslandExtensions.SystemHashes.Contains(hash))
            {
                position += new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));
                hash = IslandExtensions.GenerateHash(position);
            }

            var request = new NetworkSpawnRequest(hash, type, position, collectableItem: collectableItem);
            _networkService.Spawn(request, true);
        }

        public void MarkDestroyed(WorldObjectBase worldObject)
        {
            _worldProfile.SpawnedObjects.Remove(worldObject.Hash);
            _worldProfile.DestroyedObject.Add(worldObject.Hash);
        }

        public void UpdateHealth(WorldObjectBase worldObject, int health)
        {
            if (health == 0)
            {
                _worldProfile.ObjectHealthDictionary.Remove(worldObject.Hash);
            }

            if (!_worldProfile.ObjectHealthDictionary.TryAdd(worldObject.Hash, health))
            {
                _worldProfile.ObjectHealthDictionary[worldObject.Hash] = health;
            }
        }

        public void UpdateContainer(WorldObjectBase worldObject, ItemEntity item)
        {
            if (!_worldProfile.ObjectContainerDictionary.TryGetValue(worldObject.Hash, out var container))
            {
                if (item.Count == 0)
                {
                    return;
                }

                container = new ObjectContainer();
                _worldProfile.ObjectContainerDictionary.Add(worldObject.Hash, container);
            }

            if (item.Count == 0)
            {
                container.Items.Remove(item.Type);
            }
            else
            {
                container.Items[item.Type] = item.Count;
            }

            if (container.Items.Count == 0)
            {
                _worldProfile.ObjectContainerDictionary.Remove(worldObject.Hash);
            }
        }
    }
}