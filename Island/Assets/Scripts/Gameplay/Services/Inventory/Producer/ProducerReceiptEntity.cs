using System;
using AYellowpaper.SerializedCollections;
using Island.Gameplay.Services.Inventory.Items;

namespace Island.Gameplay.Services.Inventory.Producer
{
    [Serializable]
    public class ProducerReceiptEntity
    {
        [SerializedDictionary("Item", "Count")]
        public SerializedDictionary<InventoryItemType, int> Materials;
        public InventoryItemType ResultItemType;
        public int ResultItemCount;

        public bool IsSuitable(InventoryService inventoryService)
        {
            foreach (var material in Materials)
            {
                if (!inventoryService.IsSuitable(material.Key, material.Value))
                {
                    return false;
                }
            }

            if (!inventoryService.IsEnoughSpace(ResultItemType, ResultItemCount))
            {
                return false;
            }

            return true;
        }

        public void UseResources(InventoryService inventoryService)
        {
            foreach (var material in Materials)
            {
                inventoryService.TryRemove(material.Key, material.Value);
            }
        }
    }
}