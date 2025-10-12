using System;
using MemoryPack;
using Unity.Netcode;
using UnityEngine;

namespace Island.Gameplay.Services.Inventory.Items
{
    [Serializable, MemoryPackable]
    public partial class ItemEntity : IEquatable<ItemEntity>, INetworkSerializable
    {
        [SerializeField]
        private InventoryItemType _type;
        [SerializeField]
        private int _count = 1;

         public InventoryItemType Type
        {
            get => _type;
            set => _type = value;
        }

         public int Count
        {
            get => _count;
            set => _count = value;
        }

        public bool Equals(ItemEntity other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Type == other.Type && Count == other.Count;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ItemEntity)obj);
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