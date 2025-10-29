using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private bool _isBench;
        [SerializeField, ShowIf("_isBench")] private CraftReceipt _defaultReceipt;
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
        }

        public override async UniTask<bool> Perform(bool isJustUsed)
        {
            if (!isJustUsed)
            {
                return false;
            }

            ItemEntity[] ingredients;

            if (_isBench)
            {
                var popup = await _popup.Show(extraArgs: new object[] { Type });
                var result = await popup.WaitForResult();
                if (result == null)
                {
                    return false;
                }

                _receipt.Value = result.Value;
                ingredients = _receipt.Value.Ingredients;
            }
            else
            {
                var first = _defaultReceipt.Ingredients.FirstOrDefault(t => t.Type == _inventoryService.SelectedItem);
                if (first.Type == InventoryItemType.None)
                {
                    return false;
                }

                _receipt.Value = _defaultReceipt;
                ingredients = new ItemEntity[] { new(_inventoryService.SelectedItem, 1) };
            }

            if (_receipt.Value.Ingredients == null)
            {
                return false;
            }

            foreach (var item in ingredients)
            {
                if (!_inventoryService.IsSuitable(item))
                {
                    return false;
                }
            }

            CheckItemsForReceipt_ServerRpc(ingredients, _receipt.Value, NetworkManager.LocalClientId);

            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        private void CheckItemsForReceipt_ServerRpc(ItemEntity[] items, CraftReceipt receipt, ulong targetClientId)
        {
            for (var index = 0; index < items.Length; index++)
            {
                bool result;
                var item = items[index];
                var receiptItem = receipt.Ingredients.FirstOrDefault(t => t.Type == item.Type);
                if (receiptItem.Type == InventoryItemType.None)
                {
                    Debug.LogError($"Trying to check is item {item.Type} suitable from invalid receipt ({receipt})");
                    result = false;
                }
                else
                {
                    var maxCount = receipt.Ingredients.First(t => t.Type == item.Type).Count;
                    var exist = Container.TryGet(t => t.Type == item.Type, out var entity);
                    var overCapacity = maxCount != -1 && exist && entity.Count >= maxCount;
                    var isSuitableValue = (entity.Count + item.Count) > 0;
                    result = !overCapacity && isSuitableValue;
                }

                if (!result)
                {
                    CheckItemsForReceipt_ClientRpc(null, targetClientId.ToClientRpcParams());
                    return;
                }
            }

            CheckItemsForReceipt_ClientRpc(items, targetClientId.ToClientRpcParams());
        }

        [ClientRpc]
        private void CheckItemsForReceipt_ClientRpc(ItemEntity[] items, ClientRpcParams _)
        {
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                _inventoryService.TryRemove(item);
            }

            ChangeContainer_ServerRpc(items, NetworkManager.LocalClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChangeContainer_ServerRpc(ItemEntity[] stack, ulong targetClientId)
        {
            foreach (var item in stack)
            {
                ItemEntity resultItem;
                var index = Container.IndexOf(t => t.Type == item.Type);
                if (index == -1)
                {
                    if (item.Count >= 0)
                    {
                        resultItem = item;
                        Container.Add(item);
                    }
                    else if (item.Count == 0)
                    {
                        Debug.LogError($"Trying to add empty item {item.Type} in container {name}");
                        return;
                    }
                    else
                    {
                        Debug.LogError($"Trying to remove item {item.Type} not contained in container {name}");
                        return;
                    }
                }
                else
                {
                    var result = Container[index].Count + item.Count;
                    if (result >= 0)
                    {
                        resultItem = new ItemEntity(item.Type, result);
                        Container[index] = resultItem;
                    }
                    else
                    {
                        Debug.LogError($"Trying to remove item {item.Type} more ({item.Count}) than contained ({Container[index].Count}) in container {name}");
                        return;
                    }
                }

                _worldService.UpdateContainer(this, resultItem);
            }

            OnContainerChanged_ClientRpc(targetClientId.ToClientRpcParams());
        }

        [ClientRpc]
        private void OnContainerChanged_ClientRpc(ClientRpcParams _)
        {
            foreach (var item in _receipt.Value.Ingredients)
            {
                if (GetCount(item.Type) < item.Count)
                {
                    return;
                }
            }

            TryStartCraft_ServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void TryStartCraft_ServerRpc()
        {
            if (_completionSource?.Task.Status == UniTaskStatus.Pending)
            {
                return;
            }

            _completionSource = new UniTaskCompletionSource();

            _progressValue.Value = 0;
            _progressValue.DOValue(1, _duration).OnComplete(FinishCraft_ServerRpc).SetEase(Ease.Linear);
        }

        [ServerRpc(RequireOwnership = false)]
        private void FinishCraft_ServerRpc()
        {
            _progressValue.Value = -1;
            ChangeContainer_ServerRpc(_receipt.Value.InvertIngredients, NetworkManager.LocalClientId);
            SpawnResult(_receipt.Value.Result);

            _completionSource.TrySetResult();
        }
    }
}