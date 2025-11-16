using System;
using Island.Gameplay.Services.Inventory.Items;
using Island.Gameplay.Services.World.Objects;
using MemoryPack;
using TendedTarsier.Core.Utilities.Extensions;
using Unity.Netcode;
using UnityEngine;

namespace Island.Gameplay.Services.Server
{
    [MemoryPackable, Serializable]
    public partial struct ServerSpawnRequest : INetworkSerializable
    {
        public int Hash;
        public WorldObjectType Type;
        public Vector3 Position;
        public Quaternion Rotation;
        public ulong Owner;
        public int Health;
        public ItemEntity CollectableItem;
        public ItemEntity[] Container;
        public bool NotifyOwner;

        public ServerSpawnRequest(int hash, WorldObjectType type, Vector3 position, Quaternion rotation = default, ulong owner = 0, int health = 100, ItemEntity collectableItem = default, ItemEntity[] container = null, bool notifyOwner = false)
        {
            Hash = hash;
            Type = type;
            Position = position;
            Rotation = rotation;
            Owner = owner;
            Health = health;
            CollectableItem = collectableItem;
            Container = container;
            NotifyOwner = notifyOwner;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Hash);
            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Rotation);
            serializer.SerializeValue(ref Owner);
            serializer.SerializeValue(ref Health);
            serializer.SerializeValue(ref CollectableItem);
            serializer.SerializeArray(ref Container);
            serializer.SerializeValue(ref NotifyOwner);
        }
    }
}