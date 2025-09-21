using System;
using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Inventory.Tools;
using UnityEngine;

namespace Island.Gameplay.Services.Inventory.Items
{
    [Serializable]
    public class ItemModel : IPerformable
    {
        [field: SerializeField]
        public string Id { get; set; }

        [field: SerializeField]
        public Sprite Sprite { get; set; }

        [field: SerializeField]
        public ToolBase Tool { get; set; }

        [field: SerializeField]
        public bool IsCountable { get; set; }

        public UniTask<bool> Perform()
        {
            if (Tool == null)
            {
                return UniTask.FromResult(false);
            }

            return Tool.Perform();
        }
    }
}