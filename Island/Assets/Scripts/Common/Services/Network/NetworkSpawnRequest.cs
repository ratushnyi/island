using Island.Gameplay.Services.World.Items;
using Unity.Netcode;
using UnityEngine;

namespace Island.Common.Services.Network
{
    public struct NetworkSpawnRequest : INetworkSerializable
    {
        public WorldItemType Type;
        public Vector3 Position;
        public Quaternion Rotation;

        public NetworkSpawnRequest(WorldItemType type, Vector3 position, Quaternion rotation)
        {
            Type = type;
            Position = position;
            Rotation = rotation;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Rotation);
        }
    }
}