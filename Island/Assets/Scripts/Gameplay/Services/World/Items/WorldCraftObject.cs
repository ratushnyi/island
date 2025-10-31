using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Island.Gameplay.Configs.Craft;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using ModestTree;
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
        [SerializeField] private bool _showPopup;
        [SerializeField] private float _duration;
        [SerializeField] private WorldProgressBar _progressBar;

        [Inject] private InventoryService _inventoryService;
        [Inject] private WorldService _worldService;
        [Inject] private PanelLoader<CraftPopup> _popup;
        [Inject] private CraftConfig _craftConfig;

        private readonly NetworkVariable<float> _progressValue = new(-1);
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

            if (_showPopup)
            {
                var popup = await _popup.Show(extraArgs: new object[] { Type });
                var result = await popup.WaitForResult();
                if (result == null)
                {
                    return false;
                }

                UpdateReceipt_ServerRpc(result.Value);
                ingredients = _receipt.Value.Ingredients;
            }
            else
            {
                if (!_craftConfig.Receipts.TryGetValue(Type, out var receipts) || receipts.IsEmpty())
                {
                    return false;
                }
                var first = receipts[0].Ingredients.FirstOrDefault(t => t.Type == _inventoryService.SelectedItem);
                if (first.Type == InventoryItemType.None)
                {
                    return false;
                }

                UpdateReceipt_ServerRpc(receipts[0]);
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

            CheckItemsForReceipt_ServerRpc(ingredients, NetworkManager.LocalClientId);

            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateReceipt_ServerRpc(CraftReceipt receipt) => _receipt.Value = receipt;

        [ServerRpc(RequireOwnership = false)]
        private void CheckItemsForReceipt_ServerRpc(ItemEntity[] items, ulong targetClientId)
        {
            for (var index = 0; index < items.Length; index++)
            {
                bool result;
                var item = items[index];
                var receiptItem = _receipt.Value.Ingredients.FirstOrDefault(t => t.Type == item.Type);
                if (receiptItem.Type == InventoryItemType.None)
                {
                    Debug.LogError($"Trying to check is item {item.Type} suitable from invalid receipt)");
                    result = false;
                }
                else
                {
                    var maxCount = _receipt.Value.Ingredients.First(t => t.Type == item.Type).Count;
                    var exist = Container.TryGet(t => t.Type == item.Type, out var entity);
                    var overCapacity = maxCount != -1 && exist && entity.Count >= maxCount;
                    var isSuitableValue = (entity.Count + item.Count) > 0;
                    result = !overCapacity && isSuitableValue;
                }

                if (!result)
                {
                    PerformTransaction_ClientRpc(null, targetClientId.ToClientRpcParams());
                    return;
                }
            }

            PerformTransaction_ClientRpc(items, targetClientId.ToClientRpcParams());
        }

        [ClientRpc]
        private void PerformTransaction_ClientRpc(ItemEntity[] items, ClientRpcParams _)
        {
            if (items == null || items.Length == 0)
            {
                return;
            }

            foreach (var item in items)
            {
                _inventoryService.TryRemove(item);
            }

            PerformTransaction_ServerRpc(items);
        }

        [ServerRpc(RequireOwnership = false)]
        private void PerformTransaction_ServerRpc(ItemEntity[] items)
        {
            ApplyContainer(items);
            if (IsEnoughIngredients())
            {
                PerformCraft();
            }
        }

        private void ApplyContainer(ItemEntity[] stack)
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

        }

        private bool IsEnoughIngredients()
        {
            foreach (var item in _receipt.Value.Ingredients)
            {
                if (GetCount(item.Type) < item.Count)
                {
                    return false;
                }
            }

            return true;
        }

        private void PerformCraft()
        {
            if (_progressValue.Value >= 0)
            {
                return;
            }

            _progressValue.Value = 0;
            _progressValue.DOValue(1, _duration).OnComplete(FinishCraft).SetEase(Ease.Linear);
        }

        private void FinishCraft()
        {
            ApplyContainer(_receipt.Value.InvertIngredients);
            SpawnResult(_receipt.Value.Result);
            _progressValue.Value = -1;
            
            if (IsEnoughIngredients())
            {
                PerformCraft();
            }
        }
    }
}