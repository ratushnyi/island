using Island.Common.Services.Network;
using Island.Gameplay.Configs.Build;
using Island.Gameplay.Services.World.Objects;
using JetBrains.Annotations;
using Zenject;

namespace Island.Gameplay.Services.Build
{
    [UsedImplicitly]
    public class BuildService : NetworkServiceBase<BuildServiceFacade>
    {
        [Inject] private BuildConfig _buildConfig;
        [Inject] private AimService _aimService;

        public void Build(WorldObjectType resultType)
        {
            Facade.Build_ServerRpc(_aimService.AimObject.transform.position, resultType);
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