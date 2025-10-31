using System;
using System.Linq;
using Island.Gameplay.Services.Inventory.Items;
using TendedTarsier.Core.Utilities.Extensions;
using Unity.Netcode;
using UnityEngine;

namespace Island.Gameplay.Configs.Craft
{
    [Serializable]
    public struct CraftReceipt : INetworkSerializable
    {
        [SerializeField] private ItemEntity[] _ingredients;
        [SerializeField] private ItemEntity _result;
        public ItemEntity[] Ingredients => _ingredients;
        public ItemEntity[] InvertIngredients => _ingredients.Select(t => new ItemEntity(t.Type, -t.Count)).ToArray();
        public ItemEntity Result => _result;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _result);
            serializer.SerializeArray(ref _ingredients);
        }
    }
}