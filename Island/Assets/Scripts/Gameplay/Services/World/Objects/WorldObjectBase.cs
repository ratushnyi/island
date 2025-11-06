using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Inventory;
using TendedTarsier.Core.Utilities.Extensions;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.World.Objects
{
    public abstract class WorldObjectBase : NetworkBehaviour, IPerformable
    {
        [Inject] private WorldService _worldService;
        [field: SerializeField] public WorldObjectType Type { get; private set; }
        [field: SerializeField] public BoxCollider[] Colliders { get; private set; }
        [field: SerializeField] public MeshRenderer[] Renderers { get; private set; }
        public int Hash { get; private set; }
        public abstract string Name { get; }
        public abstract UniTask<bool> Perform(bool isJustUsed, float deltaTime);
        
        private readonly NetworkVariable<Color> _color = new(Color.white);

        private readonly Collider[] _overlapResult = new Collider[1];

        public void Init(int hash)
        {
            Hash = hash;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            SetColor(_color.Value);
            _color.AsObservable().Subscribe(SetColor).AddTo(this);
        }

        public bool IsExistCollisions(LayerMask layerMask)
        {
            for (var i = 0; i < Colliders.Length; i++)
            {
                var center = Colliders[i].transform.TransformPoint(Colliders[i].center);
                var halfExtents = Vector3.Scale(Colliders[i].size * 0.5f, Colliders[i].transform.lossyScale);
                var size = Physics.OverlapBoxNonAlloc(center, halfExtents, _overlapResult, Colliders[i].transform.rotation, layerMask);

                if (size > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void SetColor(Color color)
        {
            foreach (var aimObjectRenderer in Renderers)
            {
                aimObjectRenderer.material.color = color;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetColor_ServerRpc(Color color)
        {
            _color.Value = color;
        }

        [ServerRpc(RequireOwnership = false)]
        public void Despawn_ServerRpc()
        {
            _worldService.MarkDestroyed(this);
            NetworkObject.Despawn();
        }
    }
}