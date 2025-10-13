using AYellowpaper.SerializedCollections;
using Island.Gameplay.Services.Inventory.Items;
using Island.Gameplay.Services.Inventory.Tools;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.World.Items
{
    public class WorldItemObject : NetworkBehaviour
    {
        [SerializeField, SerializedDictionary("Tool", "Damage")]
        private SerializedDictionary<ToolItemType, int> _tools;
        [SerializeField] private Collider _collider;
        [SerializeField] private ItemEntity _itemEntity;
        [SerializeField] private WorldItemType _type;
        [Inject] private WorldService _worldService;

        public ReactiveProperty<int> Health = new();
        private readonly NetworkVariable<int> _health = new();
        private int _hash;

        public WorldItemType Type => _type;
        public string Name => _type.ToString();
        public ItemEntity ItemEntity => _itemEntity;
        public int Hash => _hash;

        public void Init(int health, int hash)
        {
            _health.Value = health;
            _hash = hash;
            Health.Value = _health.Value;
            _health.OnValueChanged += OnHealthChanged;
        }

        private void OnHealthChanged(int _, int newValue)
        {
            Health.Value = newValue;
        }

        public bool TryDestroy(ToolItemType toolType)
        {
            if (_tools.TryGetValue(toolType, out var damage))
            {
                var newHealth = _health.Value - damage;
                OnHealthChanged_ServerRpc(newHealth);
                if (newHealth == 0)
                {
                    Despawn_ServerRpc();
                    return true;
                }
            }
            
            return false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void OnHealthChanged_ServerRpc(int value)
        {
            _health.Value = value;
            _worldService.MarkHealth(this);
        }

        [ServerRpc(RequireOwnership = false)]
        private void Despawn_ServerRpc()
        {
            _worldService.MarkDestroyed(this);
            NetworkObject.Despawn();
        }

        public override void OnNetworkDespawn()
        {
            _health.OnValueChanged -= OnHealthChanged;
        }
    }
}