using System;
using System.Collections.Generic;
using Island.Gameplay.Services.Inventory.Items;
using Island.Gameplay.Services.World.Items;
using MemoryPack;
using TendedTarsier.Core.Utilities.Extensions;
using Unity.Netcode;
using UnityEngine;

namespace Island.Common.Services.Network
{
    [MemoryPackable, Serializable]
    public partial struct NetworkSpawnRequest : INetworkSerializable
    {
        public int Hash;
        public WorldObjectType Type;
        public Vector3 Position;
        public Quaternion Rotation;
        public int Health;
        public ItemEntity ResultItem;
        public List<ItemEntity> Container;

        public NetworkSpawnRequest(int hash, WorldObjectType type, Vector3 position, Quaternion rotation = default, int health = 100, ItemEntity resultItem = default, List<ItemEntity> container = null)
        {
            Hash = hash;
            Type = type;
            Position = position;
            Rotation = rotation;
            Health = health;
            ResultItem = resultItem;
            Container = container;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Hash);
            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Rotation);
            serializer.SerializeValue(ref Health);
            serializer.SerializeValue(ref ResultItem);
            serializer.SerializeList(ref Container);
        }
    }
}