using System.Collections.Generic;
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
        [SerializeField] private List<ItemEntity> _materials;
        [SerializeField] private float _duration;
        [SerializeField] private WorldProgressBar _progressBar;

        [Inject] private InventoryService _inventoryService;
        [Inject] private WorldService _worldService;

        private UniTaskCompletionSource _completionSource;
        private readonly NetworkVariable<float> _progressValue = new();

        public override string Name => Type.ToString();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _progressValue.AsObservable().Subscribe(_progressBar.SetValue).AddTo(this);

            if (IsServer)
            {
                Container.AsObservable().Subscribe(ContainerNetworkOnOnListChanged).AddTo(this);
            }
        }

        private void ContainerNetworkOnOnListChanged(NetworkListEvent<ItemEntity> changeEvent)
        {
            TryStartTransform_ServerRpc();
        }

        public override async UniTask<bool> Perform(bool isJustUsed)
        {
            if (!isJustUsed)
            {
                return false;
            }

            foreach (var item in _materials)
            {
                if (_inventoryService.SelectedItem == item.Type && _inventoryService.IsSuitable(item))
                {
                    var changedEntity = await TryChangeContainer(item);
                    var result = changedEntity.Equals(item);
                    if (result)
                    {
                        _inventoryService.TryRemove(item);
                    }

                    return result;
                }
            }

            return false;
        }

        private bool CheckMaterial()
        {
            foreach (var item in _materials)
            {
                if (GetCount(item.Type) < item.Count)
                {
                    return false;
                }
            }

            return true;
        }

        private void UseMaterials()
        {
            foreach (var item in _materials)
            {
                TryChangeContainer(new ItemEntity(item.Type, -item.Count));
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void TryStartTransform_ServerRpc()
        {
            if (_completionSource?.Task.Status == UniTaskStatus.Pending)
            {
                return;
            }

            if (!CheckMaterial())
            {
                return;
            }

            _completionSource = new UniTaskCompletionSource();

            _progressValue.Value = 0;
            _progressValue.DOValue(1, _duration).OnComplete(FinishTransform_ServerRpc).SetEase(Ease.Linear);
        }

        [ServerRpc(RequireOwnership = false)]
        private void FinishTransform_ServerRpc()
        {
            _progressValue.Value = -1;
            UseMaterials();
            SpawnResult();

            _completionSource.TrySetResult();

            TryStartTransform_ServerRpc();
        }
    }
}