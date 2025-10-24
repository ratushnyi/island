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
        [field: SerializeField] public bool IsUsable { private set; get; }
        [Inject] protected StatsService StatsService;

        public virtual bool Check()
        {
            foreach (var fee in StatFeeModel)
            {
                if (!StatsService.IsSuitable(fee.Type, fee.Value))
                {
                    return false;
                }
            }

            return true;
        }

        public virtual void Pay()
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
                result = IsUsable;
                if (IsUsable)
                {
                    Pay();
                }
            }

            return new UniTask<bool>(result);
        }
    }
}