using System;
using Island.Gameplay.Services.Inventory.Items;
using UniRx;
using Unity.Netcode;
using UnityEngine;

namespace Island.Gameplay.Services.Inventory
{
    public class WorldItemObject : NetworkBehaviour
    {
        [SerializeField] private Collider _collider;
        [SerializeField] private ItemEntity _itemEntity;

        public string Name => _itemEntity.Id;
        public ItemEntity ItemEntity => _itemEntity;

        public IObservable<WorldItemObject> OnObjectDespawn => _onObjectDespawn;
        private readonly ISubject<WorldItemObject> _onObjectDespawn = new Subject<WorldItemObject>();
        
        [ServerRpc(RequireOwnership = false)]
        public void Destroy_ServerRpc()
        {
            BroadcastDespawn_ClientRpc();
            NetworkObject.Despawn();
        }

        [ClientRpc]
        private void BroadcastDespawn_ClientRpc()
        {
            _onObjectDespawn.OnNext(null);
        }
    }
}