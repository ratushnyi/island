using Island.Gameplay.Services.Inventory.Items;
using Island.Gameplay.Services.Inventory.Tools;
using Unity.Netcode;
using UnityEngine;

namespace Island.Gameplay.Services.World.Items
{
    public class WorldItemObject : NetworkBehaviour
    {
        [SerializeField] private Collider _collider;
        [SerializeField] private ItemEntity _itemEntity;
        [SerializeField] private WorldItemType _type;
        [SerializeField] private ToolItemType _toolType;

        public WorldItemType Type => _type;
        public string Name => _type.ToString();
        public ItemEntity ItemEntity => _itemEntity;

        public bool TryPerform(ToolItemType toolType)
        {
            if (_toolType == ToolItemType.None || _toolType != toolType)
            {
                return false;
            }
            
            return true;

        }

        [ServerRpc(RequireOwnership = false)]
        public void Despawn_ServerRpc()
        {
            NetworkObject.Despawn();
        }
    }
}