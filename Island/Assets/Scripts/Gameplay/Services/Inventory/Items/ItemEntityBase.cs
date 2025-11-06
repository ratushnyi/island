using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Island.Gameplay.Configs.Stats;
using Island.Gameplay.Services.Stats;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.Inventory.Tools
{
    [CreateAssetMenu(menuName = "Item/Base", fileName = "Base")]
    public class ItemEntityBase : ScriptableObject, IPerformable
    {
        [field: SerializeField] public List<StatFeeModel> StatFeeModel { private set; get; }
        [Inject] protected StatsService StatsService;

        protected virtual void Pay()
        {
            foreach (var fee in StatFeeModel)
            {
                StatsService.TryApplyValue(fee.Type, fee.Value);
            }
        }

        public virtual UniTask<bool> Perform(bool isJustUsed, float deltaTime)
        {
            var result = false;
            if (isJustUsed)
            {
                result = true;
                Pay();
            }

            return new UniTask<bool>(result);
        }
    }
}