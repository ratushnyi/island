using Island.Common;
using Island.Gameplay.Services;
using Island.Gameplay.Settings;
using TendedTarsier.Core.Services.Modules;
using TendedTarsier.Core.Utilities.Extensions;
using UnityEngine;
using PlayerProfile = Island.Gameplay.Profiles.PlayerProfile;

namespace Island.Gameplay.Module
{
    public class IslandGameplayModuleInstaller : ModuleInstallerBase
    {
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private  CameraConfig _cameraConfig;

        protected override void InstallModuleBindings()
        {
            BindServices();
            BindProfiles();
            BindConfigs();
        }

        private void BindServices()
        {
            Container.BindService<EnergyService>();
            Container.BindService<SettingsService>();
        }

        private void BindProfiles()
        {
            Container.BindProfile<PlayerProfile>();
        }

        private void BindConfigs()
        {
            Container.Bind<PlayerConfig>().FromInstance(_playerConfig).AsSingle().NonLazy();
            Container.Bind<CameraConfig>().FromInstance(_cameraConfig).AsSingle().NonLazy();
        }
    }
}