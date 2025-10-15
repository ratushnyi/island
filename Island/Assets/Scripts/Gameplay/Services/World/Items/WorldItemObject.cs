using AYellowpaper.SerializedCollections;
using Island.Gameplay.Services.Inventory.Items;
using Island.Gameplay.Services.Inventory.Tools;
using NaughtyAttributes;
using TendedTarsier.Core.Utilities.Extensions;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.World.Items
{
    public class WorldItemObject : NetworkBehaviour
    {
        [ShowNonSerializedField] private int _hash;
        [SerializeField, SerializedDictionary("Tool", "Damage")]
        private SerializedDictionary<ToolItemType, int> _tools;
        [SerializeField, SerializedDictionary("Health", "ViewObject")]
        private SerializedDictionary<int, GameObject> _healthView;
        [SerializeField] private Collider _collider;
        [SerializeField] private ItemEntity _itemEntity;
        [SerializeField] private WorldItemType _type;
        [Inject] private WorldService _worldService;

        public ReactiveProperty<int> Health = new();
        private readonly NetworkVariable<int> _health = new();

        public WorldItemType Type => _type;
        public string Name => _type.ToString();
        public ItemEntity ItemEntity => _itemEntity;
        public int Hash => _hash;

        public void Init(int health, int hash)
        {
            _health.Value = health;
            _hash = hash;
        }

        public override void OnNetworkSpawn()
        {
            Health.Subscribe(UpdateView).AddTo(this);
            _health.AsObservable().Subscribe(Health.SetValue).AddTo(this);
            if (IsClient)
            {
                UpdateView(_health.Value);
            }
        }

        private void UpdateView(int health)
        {
            bool selected = false;
            foreach (var view in _healthView)
            {
                if (!selected && health >= view.Key)
                {
                    selected = true;
                    view.Value.SetActive(true);
                    continue;
                }
                view.Value.SetActive(false);
            }
        }

        public bool TryDestroy(ToolItemType toolType)
        {
            if (_tools.TryGetValue(toolType, out var damage))
            {
                var newHealth = _health.Value - damage;
                OnHealthChanged_ServerRpc(newHealth);
                if (newHealth <= 0)
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
    }
}