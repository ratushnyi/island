using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Inventory;
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
        [field: SerializeField] public ItemEntity ResultItem { get; private set; }
        [field: SerializeField] public WorldObjectType Type { get; private set; }
        public int Hash { get; private set; }
        public readonly NetworkVariable<int> Health = new();
        public readonly NetworkList<ItemEntity> Container = new();
        [Inject] private WorldService _worldService;
        [Inject] private InventoryService _inventoryService;
        public abstract string Name { get; }

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

            if (resultItem.Type != InventoryItemType.None)
            {
                ResultItem = resultItem;
            }
        }

        public abstract UniTask<bool> Perform(bool isJustUsed);

        protected virtual void UpdateView(int health)
        {
        }

        protected void SpawnResult()
        {
            _worldService.SpawnResultItem(this);
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

        protected bool TryChangeContainer(InventoryItemType type, int count, long targetClientId = -1)
        {
            Container.TryGet(t => t.Type == type, out var entity);

            if (entity.Count + count >= 0)
            {
                ChangeContainer_ServerRpc(type, count, targetClientId);
                return true;
            }

            return false;
        }

        [ClientRpc]
        private void ChargeItem_ClientRpc(InventoryItemType type, int count, ClientRpcParams clientRpcParams = default)
        {
            _inventoryService.TryRemove(type, count);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChangeContainer_ServerRpc(InventoryItemType type, int count, long targetClientId)
        {
            var index = Container.IndexOf(t => t.Type == type);
            var endValue = count;
            if (index >= 0)
            {
                endValue += Container[index].Count;
            }

            _worldService.UpdateContainer(this, type, endValue);

            if (targetClientId >= 0)
            {
                ChargeItem_ClientRpc(type, count, targetClientId.ToClientRpcParams());
            }

            if (index >= 0)
            {
                if (endValue > 0)
                {
                    Container[index] = new ItemEntity(type, endValue);
                }
                else
                {
                    Container.RemoveAt(index);
                }
            }
            else if (endValue > 0)
            {
                Container.Add(new ItemEntity(type, endValue));
            }
        }
    }
}