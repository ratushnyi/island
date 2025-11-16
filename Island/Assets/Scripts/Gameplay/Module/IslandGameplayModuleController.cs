using Cysharp.Threading.Tasks;
using Island.Common.Services;
using Island.Gameplay.Services.Server;
using TendedTarsier.Core.Modules.Project;
using TendedTarsier.Core.Services.Modules;
using UniRx;
using Unity.Netcode;
using Zenject;

namespace Island.Gameplay.Module
{
    public class IslandGameplayModuleController : ModuleControllerBase, IServerInitialize
    {
        [Inject] private ServerService _serverService;
        [Inject] private ModuleService _moduleService;
        [Inject] private ProjectConfig _projectConfig;

        public override async UniTask Initialize()
        {
            bool? isPlayerSpawned() => NetworkManager.Singleton.SpawnManager?.GetLocalPlayerObject()?.IsSpawned;
            await UniTask.WaitUntil(() => isPlayerSpawned().HasValue && isPlayerSpawned()!.Value);

            await base.Initialize();
        }

        public void OnNetworkInitialize()
        {
            if (!_serverService.IsServer)
            {
                _serverService.OnShutdown.Subscribe(_ => LoadMenu().Forget()).AddTo(CompositeDisposable);
            }
        }

        public async UniTaskVoid LoadMenu()
        {
            _serverService.Shutdown();
            await _moduleService.LoadModule(_projectConfig.MenuScene);
        }
    }
}