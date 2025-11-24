using System;
using TendedTarsier.Core.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Island.Gameplay.Panels.Player
{
    public class PlayerPopupPageButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _text;
        private PlayerPopupPage _page;

        public IObservable<PlayerPopupPage> ButtonObservable => _button.OnClickAsObservable(_page);

        public void Init(PlayerPopupPage page)
        {
            _page = page;
            _text.SetText(page.Name);
        }

        public void SetInteractable(bool value)
        {
            _button.interactable = value;
        }
    }
}