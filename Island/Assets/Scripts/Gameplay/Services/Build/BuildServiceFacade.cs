using Island.Gameplay.Services.World;
using Island.Gameplay.Services.World.Objects;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.Build
{
    public class BuildServiceFacade : NetworkBehaviour
    {
        [Inject] private WorldService _worldService;
        
        [ServerRpc(RequireOwnership = false)]
        public void Build_ServerRpc(Vector3 position, WorldObjectType resultType)
        {
            _worldService.Spawn(position, resultType);
        }
    }
}