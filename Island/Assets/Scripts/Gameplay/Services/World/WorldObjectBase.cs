using System.Collections.Generic;
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
        [field: SerializeField] public WorldObjectType Type { get; private set; }
        [Inject] private WorldService _worldService;
        public int Hash { get; private set; }
        public abstract string Name { get; }
        public readonly NetworkVariable<int> Health = new();
        protected readonly NetworkList<ItemEntity> Container = new();
        protected readonly NetworkVariable<ItemEntity> ResultItem = new();

        public void Init(int hash, int health, List<ItemEntity> container, ItemEntity resultItem)
        {
            Hash = hash;
            Health.Value = health;
            if (container != null)
            {
                foreach (var item in container)
                {
                    Container.Add(item);
                }
            }

            ResultItem.Value = resultItem;
        }

        public abstract UniTask<bool> Perform(bool isJustUsed);

        protected virtual void UpdateView(int health)
        {
        }

        protected void SpawnResult(ItemEntity result)
        {
            _worldService.SpawnResultItem(this, result);
        }

        public override void OnNetworkSpawn()
        {
            Health.AsObservable().Subscribe(UpdateView).AddTo(this);
            if (IsClient)
            {
                UpdateView(Health.Value);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        protected void OnHealthChanged_ServerRpc(int value)
        {
            Health.Value = value;
            _worldService.UpdateHealth(this);
        }

        [ServerRpc(RequireOwnership = false)]
        protected void Despawn_ServerRpc()
        {
            _worldService.MarkDestroyed(this);
            NetworkObject.Despawn();
        }

        protected int GetCount(InventoryItemType type)
        {
            Container.TryGet(t => t.Type == type, out var entity);

            return entity.Count;
        }
    }
}