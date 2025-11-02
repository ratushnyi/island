using Island.Gameplay.Services.World.Objects;
using JetBrains.Annotations;
using TendedTarsier.Core.Services;

namespace Island.Gameplay.Services.Build
{
    [UsedImplicitly]
    public class BuildService : ServiceBase
    {
        public void Build(WorldGroundObject groundObject)
        {
            
        }

        public bool IsSuitablePlace()
        {
            return true;
        }
    }
}