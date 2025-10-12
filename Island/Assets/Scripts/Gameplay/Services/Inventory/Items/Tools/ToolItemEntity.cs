using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.Inventory.Tools
{
    [CreateAssetMenu(menuName = "Tools/HammerTool", fileName = "HammerTool")]
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

            if (!UseResources())
            {
                return new UniTask<bool>(false);
            }

            var result = _aimService.TargetObject.Value.TryPerform(_type);
            if (result)
            {
                _aimService.TargetObject.Value.Despawn_ServerRpc();
                _inventoryService.TryPut(_aimService.TargetObject.Value);
            }
            
            return new UniTask<bool>(result);
        }
    }
}