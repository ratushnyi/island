using System.Collections.Generic;
using System.Linq;
using Island.Gameplay.Panels.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using UnityEngine;

namespace Island.Gameplay.Configs
{
    [CreateAssetMenu(menuName = "Island/InventoryConfig", fileName = "InventoryConfig")]
    public class InventoryConfig : ScriptableObject
    {
        public ItemModel this[string id] => InventoryItems.FirstOrDefault(t => t.Id == id);

        [field: SerializeField]
        public InventoryCellView InventoryCellView { get; set; }

        [field: SerializeField]
        public int InventoryCapacity { get; set; }

        [field: SerializeField]
        public List<ItemModel> InventoryItems { get; set; }
    }
}