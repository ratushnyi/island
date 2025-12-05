using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Island.Gameplay.Panels.Player.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using TendedTarsier.Core.Services.Modules;
using UnityEngine;

namespace Island.Gameplay.Configs.Inventory
{
    [CreateAssetMenu(menuName = "Island/InventoryConfig", fileName = "InventoryConfig")]
    public class InventoryConfig : ConfigBase
    {
        public ItemModel this[InventoryItemType id] => InventoryItems.FirstOrDefault(t => t.Type == id);

        [field: SerializeField]
        public InventoryCellView InventoryCellView { get; set; }

        [field: SerializeField]
        public int InventoryCapacity { get; set; }

        [field: SerializeField]
        public List<ItemStack> DefaultItems { get; set; }

        [field: SerializeField]
        public List<ItemModel> InventoryItems { get; set; }

        public override IEnumerable InjectItems()
        {
            return InventoryItems.Select(t => t.ItemEntity);
        }
    }
}