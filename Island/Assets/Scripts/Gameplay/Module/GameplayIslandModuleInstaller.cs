using Island.Common.Services;
using Island.Gameplay.Configs;
using Island.Gameplay.Configs.Stats;
using Island.Gameplay.Panels.HUD;
using Island.Gameplay.Panels.Inventory;
using Island.Gameplay.Services;
using Island.Gameplay.Services.HUD;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Stats;
using Island.Gameplay.Settings;
using TendedTarsier.Core.Services.Modules;
using TendedTarsier.Core.Utilities.Extensions;
using UnityEngine;

namespace Island.Gameplay.Module
{
    public class IslandGameplayModuleInstaller : ModuleInstallerBase
    {
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private StatsConfig _statsConfig;
        [SerializeField] private InventoryConfig _inventoryConfig;
        [SerializeField] private InventoryPanel _inventoryPanel;
        [SerializeField] private HUDPanel _hudPanel;
        [SerializeField] private Canvas _canvas;

        protected override void InstallModuleBindings()
        {
            BindServices();
            BindConfigs();
            BindPanels();
        }

        private void BindPanels()
        {
            ;Container.BindPanel<InventoryPanel>(_inventoryPanel, _canvas);
            ;Container.BindPanel<HUDPanel>(_hudPanel, _canvas);
        }

        private void BindServices()
        {
            Container.BindService<HUDService>();
            Container.BindService<InventoryService>();
            Container.BindService<StatsService>();
            Container.BindService<SettingsService>();
            Container.BindService<EnergyService>();
        }

        private void BindConfigs()
        {
            Container.Bind<StatsConfig>().FromInstance(_statsConfig).AsSingle().NonLazy();
            Container.Bind<InventoryConfig>().FromInstance(_inventoryConfig).AsSingle().NonLazy();
            Container.Bind<PlayerConfig>().FromInstance(_playerConfig).AsSingle().NonLazy();
        }
    }
}