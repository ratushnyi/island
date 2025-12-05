using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Island.Common;
using Island.Gameplay.Configs.Craft;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using Island.Gameplay.Services.World.Objects.UI;
using ModestTree;
using TendedTarsier.Core.Panels;
using TendedTarsier.Core.Utilities.Extensions;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.World.Objects
{
    public class WorldCraftObject : WorldContainerBase, ISelfPerformable
    {
        [SerializeField] private bool _showPopup;
        [SerializeField] private WorldProgressBar _progressBar;

        [Inject] private InventoryService _inventoryService;
        [Inject] private WorldService _worldService;
        [Inject] private PanelLoader<WorldCraftPopup> _popup;
        [Inject] private CraftConfig _craftConfig;

        private readonly NetworkVariable<float> _progressValue = new(-1);
        private readonly NetworkVariable<CraftReceiptEntity> _receipt = new();

        public override string Name => Type.ToString();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _progressValue.AsObservable().Subscribe(_progressBar.SetValue).AddTo(this);
        }

        public override async UniTask<bool> Perform(bool isJustUsed, float deltaTime)
        {
            if (!isJustUsed || IsBusy)
            {
                return false;
            }

            SetBusy_ServerRpc(true);

            var ingredients = Array.Empty<ItemStack>();
            if (_showPopup)
            {
                var popup = await _popup.Show(extraArgs: new object[] { Type });
                var result = await popup.WaitForResult();
                if (result.Count > 0 && result.Receipt.Ingredients.All(t => _inventoryService.IsEnough(t)))
                {
                    UpdateReceipt_ServerRpc(result.Receipt);
                    ingredients = result.Receipt.Ingredients.Select(t => new ItemStack(t.Type, t.Count * result.Count)).ToArray();
                }
            }
            else
            {
                if (!_craftConfig.Receipts.TryGetValue(Type, out var receipts))
                {
                    Debug.LogError($"Can't find receipts for type {Type}");
                }
                else if (receipts.IsEmpty())
                {
                    Debug.LogError($"Receipts list for type {Type} is empty");
                }
                else
                {
                    var receipt = receipts.Find(t => t.Entity.Ingredients[0].Type == _inventoryService.SelectedItem.Type);
                    if (receipt != null)
                    {
                        ingredients = new ItemStack[] { new(_inventoryService.SelectedItem.Type, 1) };
                        UpdateReceipt_ServerRpc(receipt.Entity);
                    }
                }
            }

            CheckItemsForReceipt_ServerRpc(ingredients, NetworkManager.LocalClientId);

            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateReceipt_ServerRpc(CraftReceiptEntity receiptEntity) => _receipt.Value = receiptEntity;

        [ServerRpc(RequireOwnership = false)]
        private void CheckItemsForReceipt_ServerRpc(ItemStack[] items, ulong targetClientId)
        {
            foreach (var item in items)
            {
                bool result;
                var receiptItem = _receipt.Value.Ingredients.FirstOrDefault(t => t.Type == item.Type);
                if (receiptItem.Type == InventoryItemType.None)
                {
                    Debug.LogError($"Trying to check is item {item.Type} suitable from invalid receipt)");
                    result = false;
                }
                else
                {
                    var maxCount = _receipt.Value.Ingredients.First(t => t.Type == item.Type).Count;
                    var exist = ContainerArray.TryGet(t => t.Type == item.Type, out var entity);
                    var overCapacity = maxCount != -1 && exist && entity.Count >= maxCount;
                    var isSuitableValue = (entity.Count + item.Count) > 0;
                    result = !overCapacity && isSuitableValue;
                }

                if (!result)
                {
                    PerformTransaction_ClientRpc(Array.Empty<ItemStack>(), targetClientId.ToClientRpcParams());
                    return;
                }
            }

            PerformTransaction_ClientRpc(items, targetClientId.ToClientRpcParams());
        }

        [ClientRpc]
        private void PerformTransaction_ClientRpc(ItemStack[] items, ClientRpcParams _)
        {
            foreach (var item in items)
            {
                _inventoryService.TryRemove(item);
            }

            PerformTransaction_ServerRpc(items);
        }

        [ServerRpc(RequireOwnership = false)]
        private void PerformTransaction_ServerRpc(ItemStack[] items)
        {
            ApplyContainer_ServerRpc(items);
            if (IsEnoughIngredients())
            {
                PerformCraft();
            }
            else
            {
                FinishCraft(false);
            }
        }

        private bool IsEnoughIngredients()
        {
            if (_receipt.Value.Ingredients == null)
            {
                return false;
            }
            
            foreach (var item in _receipt.Value.Ingredients)
            {
                ContainerArray.TryGet(t => t.Type == item.Type, out var entity);
                if (entity.Count < item.Count)
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
            _progressValue.DOValue(1, _receipt.Value.Duration).OnComplete(() => FinishCraft(true)).SetEase(Ease.Linear);
        }

        private void FinishCraft(bool success)
        {
            if (success)
            {
                ApplyContainer_ServerRpc(_receipt.Value.Ingredients.Invert());
                var position = transform.position + Vector3.up + Vector3.up;
                _worldService.Spawn(position, WorldObjectType.Collectable, _receipt.Value.Result);
                _progressValue.Value = -1;

                if (IsEnoughIngredients())
                {
                    PerformCraft();
                    return;
                }
            }

            SetBusy_ServerRpc(false);
        }
    }
}