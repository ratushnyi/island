using Cysharp.Threading.Tasks;
using Island.Gameplay.Profiles.Inventory;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using Unity.Netcode;
using Zenject;

namespace Island.Gameplay.Services.World.Objects
{
    public class WorldCollectableObject : WorldObjectBase, ISelfPerformable
    {
        [Inject] private InventoryService _inventoryService;
        [Inject] private InventoryProfile _inventoryProfile;

        private readonly NetworkVariable<InventoryItemType> _collectableItem = new();

        public override string Name => _collectableItem.Value.ToString();

        public void InitCollectable(InventoryItemType collectableItem)
        {
            _collectableItem.Value = collectableItem;
        }
        
        public override UniTask<bool> Perform(bool isJustUsed, float deltaTime)
        {
            if (!isJustUsed)
            {
                return UniTask.FromResult(false);
            }

            if (_inventoryService.TryCollect(_collectableItem.Value, 1))
            {
                Despawn_ServerRpc();
                
                _inventoryProfile.Save();
            }

            return UniTask.FromResult(true);
        }
    }
}