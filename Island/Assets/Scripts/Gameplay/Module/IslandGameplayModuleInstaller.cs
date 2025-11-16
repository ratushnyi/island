using Island.Gameplay.Configs.Aim;
using Island.Gameplay.Configs.Build;
using Island.Gameplay.Configs.Craft;
using Island.Gameplay.Configs.DateTime;
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
using Island.Gameplay.Services.Build;
using Island.Gameplay.Services.CameraInput;
using Island.Gameplay.Services.DateTime;
using Island.Gameplay.Services.HUD;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Server;
using Island.Gameplay.Services.Stats;
using Island.Gameplay.Services.World;
using Island.Gameplay.Services.World.Objects.UI;
using Island.Gameplay.Settings;
using Island.Menu.Panels.Settings;
using TendedTarsier.Core.Services.Modules;
using TendedTarsier.Core.Utilities.Extensions;
using UnityEngine;

namespace Island.Gameplay.Module
{
    public class IslandGameplayModuleInstaller : ModuleInstallerBase<IslandGameplayModuleController>
    {
        [Header("NetworkFacade")]
        [SerializeField] private ServerServiceFacade _serverServiceFacade;
        [SerializeField] private DateTimeServiceFacade _dateTimeServiceFacade;
        [SerializeField] private BuildServiceFacade _buildServiceFacade;
        [Header("System")]
        [SerializeField] private Canvas _canvas;
        [Header("Configs")]
        [SerializeField] private WorldConfig _worldConfig;
        [SerializeField] private BuildConfig _buildConfig;
        [SerializeField] private AimConfig _aimConfig;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private StatsConfig _statsConfig;
        [SerializeField] private InventoryConfig _inventoryConfig;
        [SerializeField] private CraftConfig _craftConfig;
        [SerializeField] private DateTimeConfig _dateTimeConfig;
        [Header("UI")]
        [SerializeField] private InventoryPopup _inventoryPopup;
        [SerializeField] private InputPanel _inputPanel;
        [SerializeField] private HUDPanel _hudPanel;
        [SerializeField] private PausePopup _pausePopup;
        [SerializeField] private SettingsPopup _settingsPopup;
        [SerializeField] private WorldCraftPopup _worldCraftPopup;

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
            Container.BindProfile<DateTimeProfile>();
        }

        private void BindPanels()
        {
            Container.BindPanel(_inventoryPopup, _canvas);
            Container.BindPanel(_inputPanel, _canvas);
            Container.BindPanel(_hudPanel, _canvas);
            Container.BindPanel(_pausePopup, _canvas);
            Container.BindPanel(_settingsPopup, _canvas);
            Container.BindPanel(_worldCraftPopup, _canvas);
        }

        private void BindServices()
        {
            Container.BindService<CameraInputService>();
            Container.BindService<HUDService>();
            Container.BindService<InventoryService>();
            Container.BindService<StatsService>();
            Container.BindService<AimService>();
            Container.BindService<WorldService>();
            Container.BindService<PlayerService>();
            Container.BindService<BuildService, BuildServiceFacade>(_buildServiceFacade);
            Container.BindService<DateTimeService, DateTimeServiceFacade>(_dateTimeServiceFacade);
            Container.BindService<ServerService, ServerServiceFacade>(_serverServiceFacade);
        }

        private void BindConfigs()
        {
            Container.BindConfigs(_inventoryConfig);
            Container.BindConfigs(_statsConfig);
            Container.BindConfigs(_playerConfig);
            Container.BindConfigs(_worldConfig);
            Container.BindConfigs(_craftConfig);
            Container.BindConfigs(_aimConfig);
            Container.BindConfigs(_buildConfig);
            Container.BindConfigs(_dateTimeConfig);
        }
    }
}