using System;
using System.Collections.Generic;
using System.Linq;
using Island.Gameplay.Services.Inventory.Items;
using Island.Gameplay.Services.World.Objects;
using UnityEngine;

namespace Island.Common
{
    public static class IslandExtensions
    {
        public static int AimHash = -1;
        public static int GroundHash = 0;

        public static int[] SystemHashes =
        {
            AimHash,
            GroundHash
        };

        public static bool TryGet(this IEnumerable<ItemStack> items, Func<ItemStack, bool> predicate, out ItemStack item)
        {
            item = items.FirstOrDefault(predicate);
            return item.Type != default;
        }

        public static void Populate(this IList<ItemStack> items, ItemStack oldItem, ItemStack newItem)
        {
            var index = items.IndexOf(oldItem);
            items[index] = newItem;
        }

        public static ItemStack[] Invert(this IEnumerable<ItemStack> items)
        {
            return items.Select(t => new ItemStack(t.Type, -t.Count)).ToArray();
        }

        public static int GenerateHash(this WorldObjectBase objectBase)
        {
            return GenerateHash(objectBase.transform.position, objectBase.transform.rotation);
        }

        public static int GenerateHash(Vector3 position, Quaternion rotation = default)
        {
            return HashCode.Combine(position, rotation);
        }
    }
}