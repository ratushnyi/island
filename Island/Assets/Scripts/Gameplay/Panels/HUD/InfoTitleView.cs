using TMPro;
using UnityEngine;

namespace Island.Gameplay.Panels.HUD
{
    public class InfoTitleView : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _valueTMP;
        
        public void UpdateValue(string value)
        {
            _valueTMP.SetText(value);
        }
    }
}