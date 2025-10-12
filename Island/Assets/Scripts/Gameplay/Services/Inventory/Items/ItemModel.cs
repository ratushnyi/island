using System;
using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Inventory.Tools;
using Island.Gameplay.Services.World;
using UnityEngine;

namespace Island.Gameplay.Services.Inventory.Items
{
    [Serializable]
    public class ItemModel : IPerformable
    {
        [field: SerializeField]
        public InventoryItemType Type { get; set; }

        [field: SerializeField]
        public Sprite Sprite { get; set; }

        [field: SerializeField]
        public ItemEntityBase ItemEntity { get; set; }

        [field: SerializeField]
        public bool IsCountable { get; set; }

        public UniTask<bool> Perform()
        {
            if (ItemEntity == null)
            {
                return UniTask.FromResult(false);
            }

            return ItemEntity.Perform();
        }
    }
}