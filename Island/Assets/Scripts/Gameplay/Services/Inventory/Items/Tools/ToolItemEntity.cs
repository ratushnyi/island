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
        
        private AimService _aimService;
        private InventoryService _inventoryService;

        [Inject]
        public void Construct(AimService aimService, InventoryService inventoryService)
        {
            _aimService = aimService;
            _inventoryService = inventoryService;
        }

        public override UniTask<bool> Perform()
        {
            if (_aimService.TargetObject.Value == null)
            {
                return new UniTask<bool>(false);
            }

            if (!(_aimService.TargetObject.Value is WorldItemObject itemObject))
            {
                return new UniTask<bool>(false);
            }

            if (!IsEnoughResources())
            {
                return new UniTask<bool>(false);
            }

            Pay();

            var result = itemObject.TryDestroy(_type);
            if (result)
            {
                _inventoryService.TryCollect(itemObject);
            }
            
            return new UniTask<bool>(result);
        }
    }
}