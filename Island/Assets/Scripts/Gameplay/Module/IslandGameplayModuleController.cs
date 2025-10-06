using Cysharp.Threading.Tasks;
using Island.Common.Services;
using NaughtyAttributes;
using TendedTarsier.Core.Modules.Loading;
using TendedTarsier.Core.Modules.Project;
using TendedTarsier.Core.Services.Modules;
using TendedTarsier.Core.Services.Profile;
using UniRx;
using Zenject;

namespace Island.Gameplay.Module
{
    public class IslandGameplayModuleController : ModuleControllerBase, INetworkInitialize
    {
        private ProfileService _profileService;
        private NetworkService _networkService;
        private ModuleService _moduleService;
        private ProjectConfig _projectConfig;

        [Inject]
        private void Construct(ProfileService profileService, NetworkService networkService, ModuleService moduleService, ProjectConfig projectConfig)
        {
            _profileService = profileService;
            _networkService = networkService;
            _moduleService = moduleService;
            _projectConfig = projectConfig;
        }
        
        public void OnNetworkInitialize()
        {
            if (!_networkService.IsServer)
            {
                _networkService.OnServerStopped.Subscribe(_ => LoadMenu().Forget()).AddTo(CompositeDisposable);
            }
        }

        public async UniTaskVoid LoadMenu()
        {
            await _moduleService.LoadModule(_projectConfig.MenuScene);
            _networkService.Shutdown();
        }
    }
}