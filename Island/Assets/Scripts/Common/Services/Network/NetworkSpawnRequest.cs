using System;
using Island.Gameplay.Services.World.Items;
using Unity.Netcode;
using UnityEngine;

namespace Island.Common.Services.Network
{
    [Serializable]
    public struct NetworkSpawnRequest : INetworkSerializable
    {
        public WorldItemType Type;
        public Vector3 Position;
        public Quaternion Rotation;
        public int Health;
        public int Hash;

        public NetworkSpawnRequest(WorldItemType type, Vector3 position, Quaternion rotation, int health, int hash)
        {
            Type = type;
            Position = position;
            Rotation = rotation;
            Health = health;
            Hash = hash;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Rotation);
            serializer.SerializeValue(ref Health);
            serializer.SerializeValue(ref Hash);
        }
    }
}