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
        [SerializeField]
        private StatType _statType;
        [SerializeField]
        private int _value;

        protected StatsService StatsService;

        protected bool UseResources() => StatsService.TryApplyValue(_statType, _value, true);
        protected bool IsSuitable() => StatsService.IsSuitable(_statType, _value);

        [Inject]
        public void Construct(StatsService statsService)
        {
            StatsService = statsService;
        }

        public virtual UniTask<bool> Perform()
        {
            return new UniTask<bool>(UseResources());
        }
    }
}