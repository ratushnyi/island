using System.Collections.Generic;
using System.Linq;
using Island.Gameplay.Services.Inventory.Items;
using MemoryPack;

namespace Island.Gameplay.Profiles
{
    [MemoryPackable]
    public partial class ObjectContainer
    {
        [MemoryPackAllowSerialize]
        public Dictionary<InventoryItemType, int> Items = new();

        public List<ItemEntity> ToItemsList()
        {
            return Items.Select(item => new ItemEntity(item.Key, item.Value)).ToList();
        }
    }
}