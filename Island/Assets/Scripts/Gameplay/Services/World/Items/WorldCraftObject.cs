using Cysharp.Threading.Tasks;
using DG.Tweening;
using Island.Gameplay.Configs.Craft;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using NaughtyAttributes;
using TendedTarsier.Core.Panels;
using TendedTarsier.Core.Utilities.Extensions;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.World.Items
{
    public class WorldCraftObject : WorldObjectBase
    {
        [SerializeField] private bool _simpleCraft;
        [SerializeField, ShowIf("_simpleCraft")] private CraftReceipt _defaultReceipt;
        [SerializeField] private float _duration;
        [SerializeField] private WorldProgressBar _progressBar;

        [Inject] private InventoryService _inventoryService;
        [Inject] private WorldService _worldService;
        [Inject] private PanelLoader<CraftPopup> _popup;

        private UniTaskCompletionSource _completionSource;
        private readonly NetworkVariable<float> _progressValue = new();
        private readonly NetworkVariable<CraftReceipt> _receipt = new();

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

            if (_simpleCraft)
            {
                _receipt.Value = _defaultReceipt;
            }
            else
            {
                var popup = await _popup.Show();
                _receipt.Value = await popup.WaitForResult();
            }

            foreach (var item in _receipt.Value.Materials)
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
            foreach (var item in _receipt.Value.Materials)
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
            foreach (var item in _receipt.Value.Materials)
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
            SpawnResult(_receipt.Value.Result);

            _completionSource.TrySetResult();

            TryStartTransform_ServerRpc();
        }
    }
}