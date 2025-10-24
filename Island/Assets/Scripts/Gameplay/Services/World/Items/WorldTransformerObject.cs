using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.HUD;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.World.Items
{
    public class WorldTransformerObject : WorldObjectBase
    {
        [SerializeField, SerializedDictionary("Item", "Count")]
        private SerializedDictionary<InventoryItemType, int> _material;

        [SerializeField] private float _duration;
        [SerializeField] private ProgressBar _progressBar;
        [SerializeField] private ItemEntity _resultItem;

        [Inject] private InventoryService _inventoryService;

        private readonly Dictionary<InventoryItemType, int> _loadedItems = new();
        private UniTaskCompletionSource _completionSource;

        public override string Name => Type.ToString();

        public override async UniTask<bool> Perform(bool isJustUsed)
        {
            if (_completionSource?.Task.Status == UniTaskStatus.Pending)
            {
                return false;
            }

            if (Health.Value <= 0)
            {
                return false;
            }

            if (!TryLoad(isJustUsed))
            {
                return false;
            }

            if (!Check())
            {
                return false;
            }

            await Await();

            _loadedItems.Clear();
            _inventoryService.TryCollect(_resultItem);

            return true;
        }

        private bool TryLoad(bool isJustPressed)
        {
            var result = true;
            foreach (var item in _material)
            {
                if (_loadedItems.TryGetValue(item.Key, out var count))
                {
                    if (count == item.Value)
                    {
                        continue;
                    }
                }

                if (isJustPressed)
                {
                    if (_inventoryService.SelectedItem == item.Key && _inventoryService.TryRemove(item.Key, 1))
                    {
                        if (!_loadedItems.TryAdd(item.Key, 1))
                        {
                            _loadedItems[item.Key]++;
                        }

                        if (count == item.Value)
                        {
                            continue;
                        }
                    }
                }

                result = false;
            }

            return result;
        }

        private bool Check()
        {
            if (!_inventoryService.IsEnoughSpace(_resultItem))
            {
                return false;
            }

            return true;
        }

        private async UniTask Await()
        {
            _completionSource = new UniTaskCompletionSource();
            await _progressBar.Show_ServerRpc(_duration);
            _completionSource.TrySetResult();
        }
    }
}