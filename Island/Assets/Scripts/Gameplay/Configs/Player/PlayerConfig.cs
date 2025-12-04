using Island.Gameplay.Configs.Stats;
using TendedTarsier.Core.Services.Modules;
using UnityEngine;

namespace Island.Gameplay.Settings
{
    [CreateAssetMenu(menuName = "Island/PlayerConfig", fileName = "PlayerConfig")]
    public class PlayerConfig : ConfigBase
    {
        [field: SerializeField] public Vector3 SpawnPosition { get; private set; }
        [field: SerializeField] public float JumpDelay { get; private set; } = 1f;
        [field: SerializeField] public float JumpForce { get; private set; } = 5f;
        [field: SerializeField] public float Gravity { get; private set; } = 15;
        [field: SerializeField] public float ForwardSpeed { get; private set; } = 2;
        [field: SerializeField] public float SideSpeed { get; private set; } = 0.5f;
        
        [Header("Sprint")]
        [field: SerializeField] public float SprintMultiplier { get; private set; } = 6;
        [field: SerializeField] public int SprintGetSpeed { get; private set; } = 1;
        [field: SerializeField] public int SprintFallSpeed { get; private set; } = 5;
        [field: SerializeField] public StatFeeModel SprintFee { get; set; }
    }
}