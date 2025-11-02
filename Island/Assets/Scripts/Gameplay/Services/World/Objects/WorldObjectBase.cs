using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.World.Objects
{
    public abstract class WorldObjectBase : NetworkBehaviour
    {
        [Inject] private WorldService _worldService;
        [field: SerializeField] public WorldObjectType Type { get; private set; }
        public int Hash { get; private set; }

        public abstract string Name { get; }
        public abstract UniTask<bool> Perform(bool isJustUsed);

        public void Init(int hash)
        {
            Hash = hash;
        }

        [ServerRpc(RequireOwnership = false)]
        protected void Despawn_ServerRpc()
        {
            _worldService.MarkDestroyed(this);
            NetworkObject.Despawn();
        }
    }
}