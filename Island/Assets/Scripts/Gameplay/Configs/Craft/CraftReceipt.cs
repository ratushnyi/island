using UnityEngine;

namespace Island.Gameplay.Configs.Craft
{
    [CreateAssetMenu(menuName = "Island/Craft/Receipt", fileName = "Receipt#")]
    public class CraftReceipt : ScriptableObject
    {
        public CraftReceiptEntity Entity;
    }
}