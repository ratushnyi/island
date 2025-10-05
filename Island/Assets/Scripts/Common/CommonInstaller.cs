using Island.Common.Services;
using Island.Gameplay.Profiles;
using Island.Gameplay.Profiles.Inventory;
using Island.Gameplay.Profiles.Stats;
using Island.Gameplay.Settings;
using TendedTarsier.Core.Utilities.Extensions;
using UnityEngine;
using Zenject;

namespace Island.Common
{
    public class CommonInstaller : MonoInstaller
    {
        [SerializeField] private CameraConfig _cameraConfig;
        [SerializeField] private NetworkServiceFacade _networkServiceFacade;

        public override void InstallBindings()
        {
            BindProfiles();
            BindConfigs();
            BindServices();
        }

        private void BindConfigs()
        {
            Container.Bind<CameraConfig>().FromInstance(_cameraConfig).AsSingle().NonLazy();
        }

        private void BindProfiles()
        {
            Container.BindProfile<StatsProfile>();
            Container.BindProfile<InventoryProfile>();
            Container.BindProfile<PlayerProfile>();
        }

        private void BindServices()
        {
            Container.BindService<SettingsService>();
            Container.BindService<NetworkService, NetworkServiceFacade>(_networkServiceFacade);
        }
    }
}