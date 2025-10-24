using System;
using Cysharp.Threading.Tasks;
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

        public ReactiveProperty<int> Health = new();
        private readonly NetworkVariable<int> _health = new();
        public int Hash => _hash;
        private int _hash;
        [Inject] private WorldService _worldService;

        public void Init(int health, int hash)
        {
            _health.Value = health;
            _hash = hash;
        }

        public abstract UniTask<bool> Perform(bool isJustUsed);

        protected virtual void UpdateView(int health)
        {
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

        [ServerRpc(RequireOwnership = false)]
        protected void OnHealthChanged_ServerRpc(int value)
        {
            _health.Value = value;
            _worldService.MarkHealth(this);
        }

        [ServerRpc(RequireOwnership = false)]
        protected void Despawn_ServerRpc()
        {
            _worldService.MarkDestroyed(this);
            NetworkObject.Despawn();
        }
    }
}