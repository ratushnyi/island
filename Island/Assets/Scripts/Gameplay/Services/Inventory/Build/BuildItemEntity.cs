using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Build;
using Island.Gameplay.Services.Inventory.Tools;
using Island.Gameplay.Services.World.Objects;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.Inventory.Build
{
    [CreateAssetMenu(menuName = "Item/Build", fileName = "Build")]
    public class BuildItemEntity : ItemEntityBase
    {
        [SerializeField] private WorldObjectType _resultType;
        [Inject] private AimService _aimService;
        [Inject] private BuildService _buildService;

        public WorldObjectType ResultType => _resultType;

        public override async UniTask<bool> Perform(bool isJustUsed, float deltaTime)
        {
            if (_aimService.TargetObject.Value is WorldGroundObject)
            {
                if (!await base.Perform(isJustUsed, deltaTime))
                {
                    return false;
                }
                
                if (!_buildService.IsSuitablePlace(_resultType))
                {
                    return false;
                }

                _buildService.Build(_resultType);
                return true;
            }

            return false;
        }
    }
}