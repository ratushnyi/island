using System;
using MemoryPack;
using Unity.Netcode;
using UnityEngine;

namespace Island.Gameplay.Services.Inventory.Items
{
    [Serializable, MemoryPackable]
    public partial struct ItemStack : IEquatable<ItemStack>, INetworkSerializable
    {
        [SerializeField] private InventoryItemType _type;
        [SerializeField] private int _count;

        public ItemStack(InventoryItemType type, int count)
        {
            _type = type;
            _count = count;
        }

        public InventoryItemType Type
        {
            get => _type;
        }

        public int Count
        {
            get => _count;
        }

        public bool Equals(ItemStack other)
        {
            return Type == other.Type && Count == other.Count;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Count);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _type);
            serializer.SerializeValue(ref _count);
        }
    }
}