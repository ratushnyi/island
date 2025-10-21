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
        [SerializeField] private float _duration;
        private InventoryService _inventoryService;
        public float Duration => _duration;

        [Inject]
        public void Construct(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public override UniTask<bool> Perform()
        {
            _inventoryService.TryCollect(_receipt.ResultItemType, _receipt.ResultItemCount);
            
            return new UniTask<bool>(true);
        }

        public override void Pay()
        {
            base.Pay();
            _receipt.UseResources(_inventoryService);
        }

        public override bool IsEnoughResources()
        {
            if (!base.IsEnoughResources())
            {
                return false;
            }

            if (!_receipt.IsSuitable(_inventoryService))
            {
                return false;
            }

            return true;
        }
    }
}