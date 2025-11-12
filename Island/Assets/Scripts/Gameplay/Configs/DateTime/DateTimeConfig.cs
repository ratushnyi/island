using TendedTarsier.Core.Services.Modules;
using UnityEngine;

namespace Island.Gameplay.Configs.DateTime
{
    [CreateAssetMenu(menuName = "Island/DateTimeConfig", fileName = "DateTimeConfig")]
    public class DateTimeConfig : ConfigBase
    {
        [field: SerializeField] public double Rate { get; set; }
    }
}