using Island.Common.Services;
using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Configs.Stats;
using Island.Gameplay.Configs.World;
using Island.Gameplay.Panels.HUD;
using Island.Gameplay.Panels.Inventory;
using Island.Gameplay.Panels.Pause;
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
        [SerializeField] private InventoryPanel _inventoryPanel;
        [SerializeField] private InputPanel _inputPanel;
        [SerializeField] private HUDPanel _hudPanel;
        [SerializeField] private PausePanel _pausePanel;
        [SerializeField] private SettingsPanel _settingsPanel;
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
        }

        private void BindPanels()
        {
            Container.BindPanel(_inventoryPanel, _canvas);
            Container.BindPanel<InputPanel>(_inputPanel, _canvas);
            Container.BindPanel<HUDPanel>(_hudPanel, _canvas);
            Container.BindPanel<PausePanel>(_pausePanel, _canvas);
            Container.BindPanel<SettingsPanel>(_settingsPanel, _canvas);
        }

        private void BindServices()
        {
            Container.BindService<HUDService>();
            Container.BindService<InventoryService>();
            Container.BindService<StatsService>();
            Container.BindService<EnergyService>();
            Container.BindService<AimService>();
            Container.BindService<WorldService>();
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