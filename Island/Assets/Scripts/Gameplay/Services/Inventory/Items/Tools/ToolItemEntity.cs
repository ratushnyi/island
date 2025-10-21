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

        public override async UniTask<bool> Perform(float deltaTime)
        {
            if (_aimService.TargetObject.Value == null)
            {
                return false;
            }

            if (_aimService.TargetObject.Value is not WorldItemObject itemObject)
            {
                return false;
            }

            foreach (var fee in StatFeeModel)
            {
                if (!StatsService.TrackFee(fee, deltaTime))
                {
                    return false;
                }
            }

            var result = await itemObject.Perform(_type);

            return result;
        }
    }
}