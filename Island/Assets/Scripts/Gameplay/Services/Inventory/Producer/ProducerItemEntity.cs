using System;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Inventory.Items;
using Island.Gameplay.Services.Inventory.Tools;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.Inventory.Producer
{
    [Serializable]
    public class ProducerReceiptEntity
    {
        [SerializeField, SerializedDictionary("Item", "Count")]
        public SerializedDictionary<InventoryItemType, int> Materials;

        [SerializeField] public InventoryItemType ResultItemType;
        [SerializeField] public int ResultItemCount;
    }

    [CreateAssetMenu(menuName = "Item/Producer", fileName = "Producer")]
    public class ProducerItemEntity : ItemEntityBase
    {
        [SerializeField] private ProducerReceiptEntity _receipt;

        private InventoryService _inventoryService;

        [Inject]
        public void Construct(AimService aimService, InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public override UniTask<bool> Perform()
        {
            if (!IsSuitable())
            {
                return new UniTask<bool>(false);
            }

            foreach (var material in _receipt.Materials)
            {
                if (!_inventoryService.IsSuitable(material.Key, material.Value))
                {
                    return new UniTask<bool>(false);
                }
            }

            UseResources();

            foreach (var material in _receipt.Materials)
            {
                _inventoryService.TryRemove(material.Key, material.Value);
            }

            _inventoryService.TryCollect(_receipt.ResultItemType, _receipt.ResultItemCount);

            return new UniTask<bool>(true);
        }
    }
}