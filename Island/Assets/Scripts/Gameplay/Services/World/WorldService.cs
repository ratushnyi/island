using Island.Gameplay.Configs.World;
using JetBrains.Annotations;
using TendedTarsier.Core.Services;
using Zenject;

namespace Island.Gameplay.Services.World
{
    [UsedImplicitly]
    public class WorldService : ServiceBase
    {
        private WorldConfig _worldConfig;

        [Inject]
        private void Construct(WorldConfig worldConfig)
        {
            _worldConfig = worldConfig;
        }
    }
}