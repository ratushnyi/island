using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using TendedTarsier.Core.Utilities.Extensions;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.World.Items
{
    public class WorldTransformerObject : WorldObjectBase
    {
        [SerializeField, SerializedDictionary("Item", "Count")]
        private SerializedDictionary<InventoryItemType, int> _material;

        [SerializeField] private float _duration;
        [SerializeField] private WorldProgressBar _progressBar;
        [SerializeField] private ItemEntity _resultItem;

        [Inject] private InventoryService _inventoryService;

        private UniTaskCompletionSource _completionSource;
        private readonly NetworkVariable<float> _progressValue = new();
        private readonly Dictionary<InventoryItemType, int> _loadedItems = new();

        public override string Name => Type.ToString();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            _progressValue.AsObservable().Subscribe(_progressBar.SetValue).AddTo(this);
        }

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
            _progressValue.Value = 0;
            await _progressValue.DOValue(1, _duration);
            _progressValue.Value = -1;
            _completionSource.TrySetResult();
        }
    }
}