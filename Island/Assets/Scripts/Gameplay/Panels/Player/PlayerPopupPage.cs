using UnityEngine;

namespace Island.Gameplay.Panels.Player
{
    public abstract class PlayerPopupPage : MonoBehaviour
    {
        public abstract string Name { get; }
        public abstract void Initialize();
    }
}