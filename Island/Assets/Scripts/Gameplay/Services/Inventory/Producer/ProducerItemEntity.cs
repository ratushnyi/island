using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Inventory.Tools;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.Inventory.Producer
{
    [CreateAssetMenu(menuName = "Item/Producer", fileName = "Producer")]
    public class ProducerItemEntity : ItemEntityBase
    {
        [SerializeField] private ProducerReceiptEntity _receipt;

        private InventoryService _inventoryService;

        [Inject]
        public void Construct(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public override UniTask<bool> Perform()
        {
            if (!IsSuitable())
            {
                return new UniTask<bool>(false);
            }

            if (!_receipt.IsSuitable(_inventoryService))
            {
                return new UniTask<bool>(false);
            }

            UseResources();
            _receipt.UseResources(_inventoryService);

            _inventoryService.TryCollect(_receipt.ResultItemType, _receipt.ResultItemCount);

            return new UniTask<bool>(true);
        }
    }
}