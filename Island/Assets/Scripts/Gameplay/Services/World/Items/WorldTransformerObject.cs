using System.Linq;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        private SerializedDictionary<InventoryItemType, int> _materials;

        [SerializeField] private float _duration;
        [SerializeField] private WorldProgressBar _progressBar;
        [SerializeField] private ItemEntity _resultItem;

        [Inject] private InventoryService _inventoryService;
        [Inject] private WorldService _worldService;

        private UniTaskCompletionSource _completionSource;
        private readonly NetworkVariable<float> _progressValue = new();

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

            if (Health <= 0)
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

            UseMaterials();

            _inventoryService.TryCollect(_resultItem);

            return true;
        }

        private void UseMaterials()
        {
            foreach (var item in _materials)
            {
                TryChangeContainer(item.Key, -item.Value);
            }
        }

        private bool TryLoad(bool isJustPressed)
        {
            if (!isJustPressed)
            {
                return false;
            }

            var result = true;
            foreach (var item in _materials)
            {
                var entity = Container.FirstOrDefault(t => t.Type == item.Key);
                if (entity.Count == item.Value)
                {
                    continue;
                }

                if (_inventoryService.SelectedItem == item.Key && _inventoryService.TryRemove(item.Key, 1))
                {
                    TryChangeContainer(item.Key, 1);

                    entity = Container.FirstOrDefault(t => t.Type == item.Key);
                    if (entity.Count == item.Value)
                    {
                        continue;
                    }

                    result = false;
                    break;

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
            await _progressValue.DOValue(1, _duration).SetEase(Ease.Linear);
            _progressValue.Value = -1;
            _completionSource.TrySetResult();
        }
    }
}