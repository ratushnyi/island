using System;
using Cysharp.Threading.Tasks;
using Island.Common.Services;
using Island.Common.Services.Network;
using Island.Gameplay.Services.World.Objects;
using JetBrains.Annotations;
using UniRx;
using Unity.Netcode;
using Zenject;

namespace Island.Gameplay.Services.Server
{
    [UsedImplicitly]
    public class ServerService : NetworkServiceBase<ServerServiceFacade>, IInitializable
    {
        [Inject] private DiContainer _container;

        public bool IsReady => NetworkManager.Singleton.IsApproved;
        public bool IsServer => NetworkManager.Singleton.IsServer;
        public bool IsClient => NetworkManager.Singleton.IsClient;
        public bool IsHost => NetworkManager.Singleton.IsHost;
        public IObservable<WorldObjectBase> OnWorldObjectSpawned => Facade.OnClientObjectSpawned;
        public IObservable<Unit> OnShutdown => Observable.FromEvent(t => NetworkManager.Singleton.OnPreShutdown += t, t => NetworkManager.Singleton.OnPreShutdown -= t).Merge(Facade.OnShutdown);
        public IReadOnlyReactiveProperty<bool> IsServerPaused => Facade.IsPaused;
        public string JoinCode => Facade.JoinCode.Value.ToString();
        public string ServerId => Facade.ServerId.Value.ToString();

        public void SetPaused(bool value) => Facade.SetPaused(value);
        public void Spawn(ServerSpawnRequest request, bool shouldSaveToProfile) => Facade.Spawn_ServerRpc(request, shouldSaveToProfile);

        public void Initialize()
        {
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            await UniTask.WaitUntil(() => IsReady);

            _container.ResolveAll<IServerPreInitialize>().ForEach(t => t.OnNetworkPreInitialize());
            _container.ResolveAll<IServerInitialize>().ForEach(t => t.OnNetworkInitialize());
        }

        public void Shutdown()
        {
            Facade.Shutdown_ClientRpc();
            NetworkManager.Singleton.Shutdown();
        }
    }
}