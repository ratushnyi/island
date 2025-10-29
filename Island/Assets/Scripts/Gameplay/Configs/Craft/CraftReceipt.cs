using System;
using System.Collections.Generic;
using Island.Gameplay.Services.Inventory.Items;
using TendedTarsier.Core.Utilities.Extensions;
using Unity.Netcode;
using UnityEngine;

namespace Island.Gameplay.Configs.Craft
{
    [Serializable]
    public struct CraftReceipt : INetworkSerializable
    {
        [SerializeField] private List<ItemEntity> _materials;
        [SerializeField] private ItemEntity _result;
        public List<ItemEntity> Materials => _materials;
        public ItemEntity Result => _result;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _result);
            serializer.SerializeList(ref _materials);
        }
    }
}