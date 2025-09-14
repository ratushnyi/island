using TendedTarsier.Core.Services.Modules;
using TendedTarsier.Core.Utilities.Extensions;

namespace Module
{
    public class GameplayModuleInstaller : ModuleInstallerBase<GameplayModuleConfig>
    {
        protected override void InstallModuleBindings()
        {
            Container.BindProfile<PlayerProfile>();
            Container.BindService<EnergyService>();
        }
    }
}