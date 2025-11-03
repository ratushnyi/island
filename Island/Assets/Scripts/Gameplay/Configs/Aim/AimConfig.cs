using TendedTarsier.Core.Services.Modules;
using UnityEngine;

namespace Island.Gameplay.Configs.Aim
{
    [CreateAssetMenu(menuName = "Island/AimConfig", fileName = "AimConfig")]
    public class AimConfig : ConfigBase
    {
        [field: SerializeField] public LayerMask AimMask;
        [field: SerializeField] public float AimMaxDistance = 3;
    }
}