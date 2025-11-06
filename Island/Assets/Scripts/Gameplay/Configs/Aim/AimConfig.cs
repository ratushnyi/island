using NaughtyAttributes;
using TendedTarsier.Core.Services.Modules;
using UnityEngine;

namespace Island.Gameplay.Configs.Aim
{
    [CreateAssetMenu(menuName = "Island/AimConfig", fileName = "AimConfig")]
    public class AimConfig : ConfigBase
    {
        [field: SerializeField] public Vector3 AimPosition = new(0.5f, 0.5f, 0f);
        [field: SerializeField] public LayerMask AimMask;
        [field: SerializeField] public float AimMaxDistance = 3;
        [field: SerializeField, Layer] public int AimObjectLayer;
        [field: SerializeField] public Color AimObjectSuitableColor;
        [field: SerializeField] public Color AimObjectUnsuitableColor;
    }
}