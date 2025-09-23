using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Configs.Stats;
using Island.Gameplay.Configs.World;
using Island.Gameplay.Panels.HUD;
using Island.Gameplay.Panels.Inventory;
using Island.Gameplay.Services;
using Island.Gameplay.Services.HUD;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Stats;
using Island.Gameplay.Services.World;
using Island.Gameplay.Settings;
using TendedTarsier.Core.Services.Modules;
using TendedTarsier.Core.Utilities.Extensions;
using UnityEngine;

namespace Island.Gameplay.Module
{
    public class IslandGameplayModuleInstaller : ModuleInstallerBase
    {
        [SerializeField] private WorldConfig _worldConfig;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private StatsConfig _statsConfig;
        [SerializeField] private InventoryConfig _inventoryConfig;
        [SerializeField] private InventoryPanel _inventoryPanel;
        [SerializeField] private InputPanel _inputPanel;
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
            Container.BindPanel<InventoryPanel>(_inventoryPanel, _canvas);
            Container.BindPanel<InputPanel>(_inputPanel, _canvas);
            Container.BindPanel<HUDPanel>(_hudPanel, _canvas);
        }

        private void BindServices()
        {
            Container.BindService<HUDService>();
            Container.BindService<InventoryService>();
            Container.BindService<StatsService>();
            Container.BindService<EnergyService>();
            Container.BindService<WorldService>();
        }

        private void BindConfigs()
        {
            Container.Bind<InventoryConfig>().FromInstance(_inventoryConfig).AsSingle().NonLazy();
            Container.Bind<StatsConfig>().FromInstance(_statsConfig).AsSingle().NonLazy();
            Container.Bind<PlayerConfig>().FromInstance(_playerConfig).AsSingle().NonLazy();
            Container.Bind<WorldConfig>().FromInstance(_worldConfig).AsSingle().NonLazy();
        }
    }
}