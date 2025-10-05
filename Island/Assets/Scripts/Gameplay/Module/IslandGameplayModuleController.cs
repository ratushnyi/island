using Cysharp.Threading.Tasks;
using Island.Common.Services;
using NaughtyAttributes;
using TendedTarsier.Core.Services.Modules;
using TendedTarsier.Core.Services.Profile;
using UniRx;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Module
{
    public class IslandGameplayModuleController : ModuleControllerBase, INetworkInitialize
    {
        [SerializeField, Scene] private string _menuScene = "Menu";
        
        private ProfileService _profileService;
        private NetworkService _networkService;
        private ModuleService _moduleService;

        [Inject]
        private void Construct(ProfileService profileService, NetworkService networkService, ModuleService moduleService)
        {
            _profileService = profileService;
            _networkService = networkService;
            _moduleService = moduleService;
        }
        
        public void OnNetworkInitialize()
        {
            if (!_networkService.IsServer)
            {
                _networkService.OnServerStopped.Subscribe(_ => LoadMenu()).AddTo(CompositeDisposable);
            }
        }

        public void LoadMenu()
        {
            _profileService.SaveAll();
            _networkService.Shutdown();
            _moduleService.LoadModule(_menuScene).Forget();
        }
    }
}