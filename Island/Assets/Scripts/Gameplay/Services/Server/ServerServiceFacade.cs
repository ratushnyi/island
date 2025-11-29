using System;
using Island.Common.Services;
using Island.Gameplay.Configs.World;
using Island.Gameplay.Profiles;
using Island.Gameplay.Services.World.Objects;
using TendedTarsier.Core.Modules.Project;
using TendedTarsier.Core.Utilities.Extensions;
using UniRx;
using Unity.Collections;
using Unity.Netcode;
using Zenject;

namespace Island.Gameplay.Services.Server
{
    public class ServerServiceFacade : NetworkBehaviour
    {
        public IObservable<WorldObjectBase> OnClientObjectSpawned => _onClientObjectSpawned;
        private readonly ISubject<WorldObjectBase> _onClientObjectSpawned = new Subject<WorldObjectBase>();
        public IObservable<Unit> OnShutdown => _onShutdown;
        private readonly ISubject<Unit> _onShutdown = new Subject<Unit>();

        public IReadOnlyReactiveProperty<bool> IsPaused => _isPausedNetwork.AsReactiveProperty();
        private readonly NetworkVariable<bool> _isPausedNetwork = new();

        public IReadOnlyReactiveProperty<FixedString32Bytes> ServerId => _serverId.AsReactiveProperty();
        private readonly NetworkVariable<FixedString32Bytes> _serverId = new();

        public IReadOnlyReactiveProperty<FixedString32Bytes> JoinCode => _joinCode.AsReactiveProperty();
        private readonly NetworkVariable<FixedString32Bytes> _joinCode = new();

        [Inject] private WorldConfig _worldConfig;
        [Inject] private WorldProfile _worldProfile;
        [Inject] private MatchmakingService _matchmakingService;
        [Inject] private ProjectProfile _projectProfile;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsServer)
            {
                return;
            }

            _serverId.Value = _projectProfile.ServerId;
            _joinCode.Value = _matchmakingService.JoinCode;
        }

        public void SetPaused(bool value)
        {
            if (!IsServer)
            {
                return;
            }

            _isPausedNetwork.Value = value;
        }

        [ClientRpc]
        private void OnObjectSpawned_ClientRpc(ulong objectId, ClientRpcParams _)
        {
            _onClientObjectSpawned.OnNext(NetworkManager.SpawnManager.SpawnedObjects[objectId].GetComponent<WorldObjectBase>());
        }

        [ServerRpc(RequireOwnership = false)]
        public void Spawn_ServerRpc(ServerSpawnRequest request, bool shouldSaveToProfile)
        {
            if (shouldSaveToProfile)
            {
                _worldProfile.SpawnedObjects.Add(request.Hash, request);
            }

            var worldObject = Instantiate(_worldConfig.WorldObjects[request.Type], request.Position, request.Rotation);
            worldObject.NetworkObject.SpawnWithOwnership(request.Owner);
            worldObject.Init(request.Hash);
            
            if (worldObject is WorldDestroyableObject worldDestroyableObject)
            {
                worldDestroyableObject.InitHealth(request.Health);
            }
            
            if (worldObject is WorldCollectableObject worldCollectableObject)
            {
                worldCollectableObject.InitCollectable(request.CollectableItem);
            }
            
            if (worldObject is WorldContainerBase worldContainerBase)
            {
                worldContainerBase.InitContainer(request.Container);
            }

            worldObject.NetworkObject.TrySetParent(NetworkObject);

            if (request.NotifyOwner)
            {
                OnObjectSpawned_ClientRpc(worldObject.NetworkObject.NetworkObjectId, request.Owner.ToClientRpcParams());
            }
        }

        [ClientRpc]
        public void Shutdown_ClientRpc(ClientRpcParams _ = default)
        {
            _onShutdown.OnNext(Unit.Default);
        }
    }
}