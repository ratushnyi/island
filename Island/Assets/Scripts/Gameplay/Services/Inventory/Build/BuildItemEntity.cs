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
        [Inject] private AimService _aimService;
        [Inject] private BuildService _buildService;

        public override async UniTask<bool> Perform(bool isJustUsed, float deltaTime)
        {
            var result = true;
            foreach (var fee in StatFeeModel)
            {
                if (isJustUsed)
                {
                    fee.Deposit = 0;
                }

                if (!StatsService.TrackFee(fee, deltaTime))
                {
                    result = false;
                }
            }

            if (_aimService.TargetObject.Value is WorldGroundObject groundObject)
            {
                if (!_buildService.IsSuitablePlace())
                {
                    result = false;
                }
                else
                {
                    _buildService.Build(groundObject);
                }
            }
            else
            {
                result = false;
            }

            

            return result;
        }
    }
}