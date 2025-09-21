using System.Collections.Generic;
using System.Linq;
using Island.Gameplay.Panels.HUD;
using UnityEngine;

namespace Island.Gameplay.Configs.Stats
{
    [CreateAssetMenu(menuName = "Island/StatsConfig", fileName = "StatsConfig")]
    public class StatsConfig : ScriptableObject
    {
        public StatModel this[StatType type] => StatsList.FirstOrDefault(t => t.StatType == type);

        [field: SerializeField]
        public StatBarView StatBarView { get; set; }

        [field: SerializeField]
        public List<StatModel> StatsList { get; set; }

        [field: SerializeField]
        public List<StatFeeConditionModel> StatsFeeConditionalList { get; set; }
    }
}