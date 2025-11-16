using NaughtyAttributes;
using TendedTarsier.Core.Services.Modules;
using UnityEngine;

namespace Island.Gameplay.Configs.DateTime
{
    [CreateAssetMenu(menuName = "Island/DateTimeConfig", fileName = "DateTimeConfig")]
    public class DateTimeConfig : ConfigBase
    {
        [field: SerializeField] public int StartYear { get; set; } = 1998;
        [field: SerializeField, MinValue(1), MaxValue(12)] public int StartMonth { get; set; } = 5;
        [field: SerializeField, MinValue(1), MaxValue(31)] public int StartDate { get; set; } = 3;
        [field: SerializeField, Min(0.1f)] public float DayDuration { get; set; } = 60f;
        [field: SerializeField, Range(-90f, 90f)] public float Latitude { get; set; } = 50f;
    }
}