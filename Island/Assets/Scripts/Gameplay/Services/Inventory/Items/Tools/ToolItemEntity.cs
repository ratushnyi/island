using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.World.Items;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.Inventory.Tools
{
    [CreateAssetMenu(menuName = "Item/Tool", fileName = "Tool")]
    public class ToolItemEntity : ItemEntityBase
    {
        [SerializeField] private ToolItemType _type;
        [Inject] private AimService _aimService;

        public override async UniTask<bool> Perform(bool isJustStarted, float deltaTime)
        {
            var result = true;
            foreach (var fee in StatFeeModel)
            {
                if (isJustStarted)
                {
                    fee.Deposit = 0;
                }

                if (!StatsService.TrackFee(fee, deltaTime))
                {
                    result = false;
                }
            }

            if (_aimService.TargetObject.Value == null)
            {
                result = false;
            }

            if (_aimService.TargetObject.Value is WorldItemObject itemObject)
            {
                result = await itemObject.Perform(_type, result);
            }
            else
            {
                result = false;
            }


            return result;
        }
    }
}