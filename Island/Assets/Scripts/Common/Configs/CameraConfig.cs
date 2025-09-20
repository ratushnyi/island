using UnityEngine;

namespace Island.Gameplay.Settings
{
    [CreateAssetMenu(menuName = "Island/CameraConfig", fileName = "CameraConfig")]
    public class CameraConfig : ScriptableObject
    {
        [field: SerializeField] public float FovSprintModifier { get; private set; } = 1.5f;
        [field: SerializeField] public Vector2 PitchLimits { get; private set; } = new(-85f, 85f);
        [field: SerializeField] public int DefaultFov { get; private set; } = 75;
        [field: SerializeField] public int DefaultCameraSensitivity { get; private set; } = 100;
    }
}