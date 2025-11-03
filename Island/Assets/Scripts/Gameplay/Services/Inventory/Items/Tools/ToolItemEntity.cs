using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Inventory.Tools;
using Island.Gameplay.Services.World.Objects;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.Inventory.Items.Tools
{
    [CreateAssetMenu(menuName = "Item/Tool", fileName = "Tool")]
    public class ToolItemEntity : ItemEntityBase
    {
        [Inject] private AimService _aimService;

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

            if (_aimService.TargetObject.Value is WorldDestroyableObject itemObject)
            {
                if (result)
                {
                    result = await itemObject.Perform(isJustUsed, deltaTime);
                }
                else
                {
                    itemObject.Reset();
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