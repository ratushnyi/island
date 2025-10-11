using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.Inventory.Tools
{
    [CreateAssetMenu(menuName = "Tools/HammerTool", fileName = "HammerTool")]
    public class HammerTool : ToolBase
    {
        private AimService _aimService;

        [Inject]
        public void Construct(AimService aimService)
        {
            _aimService = aimService;
        }

        public override UniTask<bool> Perform()
        {
            if (_aimService.TargetObject.Value == null)
            {
                return new UniTask<bool>(false);
            }

            var targetPosition = _aimService.TargetObject;
            if (!UseResources())
            {
                return new UniTask<bool>(false);
            }

            targetPosition.Value.Destroy_ServerRpc();
            return new UniTask<bool>(true);
        }
    }
}