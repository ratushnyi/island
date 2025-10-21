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

        public virtual void Pay()
        {
            foreach (var fee in StatFeeModel)
            {
                StatsService.TryApplyValue(fee.Type, fee.Value, true);
            }
        }

        public virtual UniTask<bool> Perform(float deltaTime)
        {
            if (IsUsable)
            {
                Pay();
            }

            return new UniTask<bool>(IsUsable);
        }
    }
}