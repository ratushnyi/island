using UnityEngine;

namespace Island.Gameplay.Settings
{
    [CreateAssetMenu(menuName = "Island/PlayerConfig", fileName = "PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [field: SerializeField] public int JumpForce { get; private set; } = 5;
        [field: SerializeField] public int Gravity { get; private set; } = 1;
        [field: SerializeField] public int WalkSpeed { get; private set; } = 5;
        
        [Header("Sprint")]
        [field: SerializeField] public int SprintCost { get; private set; } = 10;
        [field: SerializeField] public int SprintSpeed { get; private set; } = 10;
        [field: SerializeField] public int SprintGetSpeed { get; private set; } = 1;
        [field: SerializeField] public int SprintFallSpeed { get; private set; } = 20;
    }
}