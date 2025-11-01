using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Build;
using Island.Gameplay.Services.Inventory.Tools;
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

            if (!_buildService.IsSuitablePlace())
            {
                result = false;
            }
            
            _buildService.Build();

            return result;
        }
    }
}