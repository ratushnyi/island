using Island.Gameplay.Configs.Stats;
using TendedTarsier.Core.Services.Modules;
using UnityEngine;

namespace Island.Gameplay.Settings
{
    [CreateAssetMenu(menuName = "Island/PlayerConfig", fileName = "PlayerConfig")]
    public class PlayerConfig : ConfigBase
    {
        [field: SerializeField] public Vector3 SpawnPosition { get; private set; }
        [field: SerializeField] public float JumpForce { get; private set; } = 5f;
        [field: SerializeField] public float Gravity { get; private set; } = 15;
        [field: SerializeField] public int WalkSpeed { get; private set; } = 5;
        
        [Header("Sprint")]
        [field: SerializeField] public int SprintSpeed { get; private set; } = 10;
        [field: SerializeField] public int SprintGetSpeed { get; private set; } = 1;
        [field: SerializeField] public int SprintFallSpeed { get; private set; } = 20;
        [field: SerializeField] public StatFeeModel SprintFee { get; set; }
    }
}