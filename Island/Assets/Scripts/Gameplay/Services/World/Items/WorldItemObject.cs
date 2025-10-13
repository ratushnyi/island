using Island.Gameplay.Services.Inventory.Items;
using Island.Gameplay.Services.Inventory.Tools;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.World.Items
{
    public class WorldItemObject : NetworkBehaviour
    {
        [SerializeField] private Collider _collider;
        [SerializeField] private ItemEntity _itemEntity;
        [SerializeField] private WorldItemType _type;
        [SerializeField] private ToolItemType _toolType;
        private int _hash;
        private WorldService _worldService;

        public WorldItemType Type => _type;
        public string Name => _type.ToString();
        public ItemEntity ItemEntity => _itemEntity;
        public int Hash => _hash;

        [Inject]
        private void Construct(WorldService worldService)
        {
            _worldService = worldService;
        }

        public void Init(int hash)
        {
            _hash = hash;
        }

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
            _worldService.MarkDestroyed(this);
            NetworkObject.Despawn();
        }
    }
}