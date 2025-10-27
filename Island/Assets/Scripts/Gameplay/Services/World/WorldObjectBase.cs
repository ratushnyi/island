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
        [SerializeField] private int _containerCapacity = 1;
        [SerializeField] private int _containerStackCapacity = 1;
        [field: SerializeField] public ItemEntity ResultItem { get; private set; }
        [field: SerializeField] public WorldObjectType Type { get; private set; }

        [Inject] private WorldService _worldService;

        public int Hash { get; private set; }
        public abstract string Name { get; }
        public readonly NetworkList<ItemEntity> Container = new();
        public readonly NetworkVariable<int> Health = new();
        private readonly ISubject<ItemEntity> _onContainerChanged = new Subject<ItemEntity>();

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

        protected UniTask<ItemEntity> TryChangeContainer(ItemEntity item)
        {
            Container.TryGet(t => t.Type == item.Type, out var entity);

            if (entity.Count + item.Count >= 0)
            {
                ChangeContainer_ServerRpc(item, NetworkManager.LocalClientId);

                return _onContainerChanged.First().ToUniTask();
            }

            return UniTask.FromResult(default(ItemEntity));
        }

        [ClientRpc]
        private void OnContainerChanged_ClientRpc(ItemEntity item, ClientRpcParams _ = default)
        {
            onContainerChanged().Forget();

            async UniTaskVoid onContainerChanged()
            {
                await UniTask.Yield();
                _onContainerChanged.OnNext(item);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChangeContainer_ServerRpc(ItemEntity item, ulong targetClientId)
        {
            var index = Container.IndexOf(t => t.Type == item.Type);
            var overCapacity = item.Count > 0 && (_containerCapacity == 0 || (index >= 0 && (Container.Count >= _containerCapacity || Container[index].Count >= _containerStackCapacity)));
            if (overCapacity)
            {
                OnContainerChanged_ClientRpc(default, targetClientId.ToClientRpcParams());
                return;
            }

            OnContainerChanged_ClientRpc(item, targetClientId.ToClientRpcParams());

            _worldService.UpdateContainer(this, item);

            if (index >= 0)
            {
                var endValue = item.Count + Container[index].Count;
                if (endValue > 0)
                {
                    Container[index] = new ItemEntity(item.Type, endValue);
                }
                else
                {
                    Container.RemoveAt(index);
                }
            }
            else if (item.Type != InventoryItemType.None && item.Count > 0)
            {
                Container.Add(item);
            }
        }
    }
}