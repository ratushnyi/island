using JetBrains.Annotations;
using TendedTarsier.Core.Services;

namespace Island.Gameplay.Services.Build
{
    [UsedImplicitly]
    public class BuildService : ServiceBase
    {
        public void Build()
        {
            
        }

        public bool IsSuitablePlace()
        {
            return true;
        }
    }
}