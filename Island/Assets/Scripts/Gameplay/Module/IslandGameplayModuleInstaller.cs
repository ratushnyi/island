using Island.Common.Services;
using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Configs.Stats;
using Island.Gameplay.Configs.World;
using Island.Gameplay.Panels.HUD;
using Island.Gameplay.Panels.Inventory;
using Island.Gameplay.Panels.Pause;
using Island.Gameplay.Player;
using Island.Gameplay.Profiles;
using Island.Gameplay.Profiles.Inventory;
using Island.Gameplay.Profiles.Stats;
using Island.Gameplay.Services;
using Island.Gameplay.Services.HUD;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Stats;
using Island.Gameplay.Services.World;
using Island.Gameplay.Settings;
using Island.Menu.Panels.Settings;
using TendedTarsier.Core.Services.Modules;
using TendedTarsier.Core.Utilities.Extensions;
using UnityEngine;

namespace Island.Gameplay.Module
{
    public class IslandGameplayModuleInstaller : ModuleInstallerBase<IslandGameplayModuleController>
    {
        [SerializeField] private NetworkServiceFacade _networkServiceFacade;
        [SerializeField] private WorldConfig _worldConfig;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private StatsConfig _statsConfig;
        [SerializeField] private InventoryConfig _inventoryConfig;
        [SerializeField] private InventoryPopup _inventoryPopup;
        [SerializeField] private InputPanel _inputPanel;
        [SerializeField] private HUDPanel _hudPanel;
        [SerializeField] private PausePopup _pausePopup;
        [SerializeField] private SettingsPopup _settingsPopup;
        [SerializeField] private Canvas _canvas;

        protected override void InstallModuleBindings()
        {
            BindServices();
            BindConfigs();
            BindPanels();
            BindProfiles();
        }

        private void BindProfiles()
        {
            Container.BindProfile<StatsProfile>();
            Container.BindProfile<InventoryProfile>();
            Container.BindProfile<PlayerProfile>();
            Container.BindProfile<WorldProfile>();
        }

        private void BindPanels()
        {
            Container.BindPanel(_inventoryPopup, _canvas);
            Container.BindPanel(_inputPanel, _canvas);
            Container.BindPanel(_hudPanel, _canvas);
            Container.BindPanel(_pausePopup, _canvas);
            Container.BindPanel(_settingsPopup, _canvas);
        }

        private void BindServices()
        {
            Container.BindService<HUDService>();
            Container.BindService<InventoryService>();
            Container.BindService<StatsService>();
            Container.BindService<EnergyService>();
            Container.BindService<AimService>();
            Container.BindService<WorldService>();
            Container.BindService<PlayerService>();
            Container.BindService<NetworkServiceBridge, NetworkServiceFacade>(_networkServiceFacade);
        }

        private void BindConfigs()
        {
            Container.BindConfigs(_inventoryConfig);
            Container.BindConfigs(_statsConfig);
            Container.BindConfigs(_playerConfig);
            Container.BindConfigs(_worldConfig);
        }
    }
}