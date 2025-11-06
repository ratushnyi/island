using Island.Gameplay.Configs.Build;
using Island.Gameplay.Services.World;
using Island.Gameplay.Services.World.Objects;
using JetBrains.Annotations;
using TendedTarsier.Core.Services;
using Zenject;

namespace Island.Gameplay.Services.Build
{
    [UsedImplicitly]
    public class BuildService : ServiceBase
    {
        [Inject] private BuildConfig _buildConfig;
        [Inject] private AimService _aimService;
        [Inject] private WorldService _worldService;
        
        public void Build(WorldObjectType resultType)
        {
            _worldService.Spawn(_aimService.AimObject.transform.position, resultType);
        }

        public bool IsSuitablePlace(WorldObjectType resultType)
        {
            if (_aimService.AimObject == null || _aimService.AimObject.Type != resultType)
            {
                return false;
            }

            return !_aimService.AimObject.IsExistCollisions(_buildConfig.BuildMask);
        }
    }
}