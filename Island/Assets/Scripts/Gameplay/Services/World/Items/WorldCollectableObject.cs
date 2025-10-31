using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Inventory;
using Zenject;

namespace Island.Gameplay.Services.World.Items
{
    public class WorldCollectableObject : WorldObjectBase
    {
        [Inject] private InventoryService _inventoryService;

        private UniTaskCompletionSource _completionSource;

        public override string Name => ResultItem.Value.Type.ToString();


        public override UniTask<bool> Perform(bool isJustUsed)
        {
            if (!isJustUsed)
            {
                return UniTask.FromResult(false);
            }

            if (_inventoryService.TryCollect(ResultItem.Value))
            {
                Despawn_ServerRpc();
            }

            return UniTask.FromResult(true);
        }
    }
}