using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using Unity.Netcode;
using Zenject;

namespace Island.Gameplay.Services.World.Objects
{
    public class WorldCollectableObject : WorldObjectBase, ISelfPerformable
    {
        [Inject] private InventoryService _inventoryService;

        private readonly NetworkVariable<ItemEntity> _collectableItem = new();

        public override string Name => _collectableItem.Value.Type.ToString();

        public void InitCollectable(ItemEntity collectableItem)
        {
            _collectableItem.Value = collectableItem;
        }
        
        public override UniTask<bool> Perform(bool isJustUsed, float deltaTime)
        {
            if (!isJustUsed)
            {
                return UniTask.FromResult(false);
            }

            if (_inventoryService.TryCollect(_collectableItem.Value))
            {
                Despawn_ServerRpc();
            }

            return UniTask.FromResult(true);
        }
    }
}