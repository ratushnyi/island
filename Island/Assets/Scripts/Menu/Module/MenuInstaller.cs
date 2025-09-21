using Island.Common.Services;
using Island.Gameplay.Profiles;
using Island.Gameplay.Profiles.Inventory;
using Island.Gameplay.Profiles.Stats;
using Island.Gameplay.Settings;
using Island.Menu.Panels.SettingsPanel;
using TendedTarsier.Core.Utilities.Extensions;
using UnityEngine;
using Zenject;

namespace Island.Menu.Module
{
    public class MenuInstaller : MonoInstaller
    {
        [SerializeField] private CameraConfig _cameraConfig;
        [SerializeField] private SettingsPanel _settingsPanel;
        [SerializeField] private Canvas _canvas;

        public override void InstallBindings()
        {
            BindServices();
            BindConfigs();
            BindPanels();
            BindProfiles();
        }

        private void BindServices()
        {
            Container.BindService<SettingsService>();
        }

        private void BindConfigs()
        {
            Container.Bind<CameraConfig>().FromInstance(_cameraConfig).AsSingle().NonLazy();
        }

        private void BindPanels()
        {
            Container.BindPanel<SettingsPanel>(_settingsPanel, _canvas);
        }

        private void BindProfiles()
        {
            Container.BindProfile<StatsProfile>();
            Container.BindProfile<InventoryProfile>();
            Container.BindProfile<PlayerProfile>();
        }
    }
}