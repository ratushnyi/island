using System;
using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Inventory.Tools;
using UnityEngine;

namespace Island.Gameplay.Services.Inventory.Items
{
    [Serializable]
    public class ItemModel
    {
        [field: SerializeField] public InventoryItemType Type { get; set; }
        [field: SerializeField] public Sprite Sprite { get; set; }
        [field: SerializeField] public ItemEntityBase ItemEntity { get; set; }
        [field: SerializeField] public bool IsCountable { get; set; }
        [field: SerializeField] public bool IsDisposable { private set; get; }
        [field: SerializeField] public bool IsBuildable { private set; get; }
    }
}