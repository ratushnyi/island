using System;
using System.Linq;
using Island.Gameplay.Services.Inventory.Items;
using TendedTarsier.Core.Utilities.Extensions;
using Unity.Netcode;
using UnityEngine;

namespace Island.Gameplay.Configs.Craft
{
    [Serializable]
    public struct CraftReceiptEntity : INetworkSerializable
    {
        [SerializeField] private ItemStack[] _ingredients;
        [SerializeField] private InventoryItemType _result;
        [SerializeField] private float _duration;
        public ItemStack[] Ingredients => _ingredients;
        public InventoryItemType Result => _result;
        public float Duration => _duration;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _result);
            serializer.SerializeValue(ref _duration);
            serializer.SerializeArray(ref _ingredients);
        }
    }
}