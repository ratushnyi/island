using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Inventory.Items;
using Island.Gameplay.Services.World.Items;
using TendedTarsier.Core.Utilities.Extensions;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.World
{
    public abstract class WorldObjectBase : NetworkBehaviour
    {
        [field: SerializeField] public WorldObjectType Type { get; set; }
        public int CombineHash => HashCode.Combine(Type, transform.position, transform.rotation);
        public abstract string Name { get; }

        public int Health => _health.Value;
        public List<ItemEntity> Container => _container.AsNativeArray().ToList();
        
        private readonly NetworkVariable<int> _health = new();
        private readonly NetworkList<ItemEntity> _container = new();
        public int Hash => _hash;
        private int _hash;
        [Inject] private WorldService _worldService;

        public void Init(int hash, int health, List<ItemEntity> container)
        {
            _hash = hash;
            _health.Value = health;
            if (container != null)
            {
                foreach (var item in container)
                {
                    _container.Add(item);
                }
            }
        }

        public abstract UniTask<bool> Perform(bool isJustUsed);

        protected virtual void UpdateView(int health)
        {
        }

        public override void OnNetworkSpawn()
        {
            _health.AsObservable().Subscribe(UpdateView).AddTo(this);
            if (IsClient)
            {
                UpdateView(_health.Value);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        protected void OnHealthChanged_ServerRpc(int value)
        {
            _health.Value = value;
            _worldService.UpdateHealth(this);
        }

        [ServerRpc(RequireOwnership = false)]
        protected void Despawn_ServerRpc()
        {
            _worldService.MarkDestroyed(this);
            NetworkObject.Despawn();
        }

        protected bool TryChangeContainer(InventoryItemType type, int count)
        {
            _container.TryGet(t => t.Type == type, out var entity);

            if (entity.Count + count >= 0)
            {
                ChangeContainer_ServerRpc(type, count);
                return true;
            }

            return false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChangeContainer_ServerRpc(InventoryItemType type, int count)
        {
            var index = _container.IndexOf(t => t.Type == type);
            int endValue = count;
            if (index >= 0)
            {
                endValue = _container[index].Count + count;
                if (endValue > 0)
                {
                    _container[index] = new ItemEntity(type, endValue);
                }
                else
                {
                    _container.RemoveAt(index);
                }
            }
            else if (count > 0)
            {
                _container.Add(new ItemEntity(type, endValue));
            }

            _worldService.UpdateContainer(this, type, endValue);
        }
    }
}