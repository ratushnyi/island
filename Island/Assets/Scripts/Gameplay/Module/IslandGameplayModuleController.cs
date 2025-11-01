using Cysharp.Threading.Tasks;
using Island.Common.Services;
using TendedTarsier.Core.Modules.Project;
using TendedTarsier.Core.Services.Modules;
using TendedTarsier.Core.Services.Profile;
using UniRx;
using Unity.Netcode;
using Zenject;

namespace Island.Gameplay.Module
{
    public class IslandGameplayModuleController : ModuleControllerBase, INetworkInitialize
    {
        private NetworkService _networkService;
        private ModuleService _moduleService;
        private ProjectConfig _projectConfig;

        [Inject]
        private void Construct(ProfileService profileService, NetworkService networkService, ModuleService moduleService, ProjectConfig projectConfig)
        {
            _networkService = networkService;
            _moduleService = moduleService;
            _projectConfig = projectConfig;
        }

        public override async UniTask Initialize()
        {
            bool? isPlayerSpawned() => NetworkManager.Singleton.SpawnManager?.GetLocalPlayerObject()?.IsSpawned;
            await UniTask.WaitUntil(() => isPlayerSpawned().HasValue && isPlayerSpawned()!.Value);
            
            await base.Initialize();
        }

        public void OnNetworkInitialize()
        {
            if (!_networkService.IsServer)
            {
                _networkService.OnShutdown.Subscribe(_ => LoadMenu().Forget()).AddTo(CompositeDisposable);
            }
        }

        public async UniTaskVoid LoadMenu()
        {
            _networkService.Shutdown();
            await _moduleService.LoadModule(_projectConfig.MenuScene);
        }
    }
}