using TendedTarsier.Core.Services.Modules;
using UnityEngine;

namespace Island.Gameplay.Configs.Build
{
    [CreateAssetMenu(menuName = "Island/BuildConfig", fileName = "BuildConfig")]
    public class BuildConfig : ConfigBase
    {
        [field: SerializeField] public LayerMask BuildMask;
    }
}