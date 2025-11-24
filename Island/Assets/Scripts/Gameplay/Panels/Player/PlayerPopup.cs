using System.Collections.Generic;
using TendedTarsier.Core.Panels;
using UniRx;
using UnityEngine;

namespace Island.Gameplay.Panels.Player
{
    public class PlayerPopup : PopupBase
    {
        [SerializeField] private List<PlayerPopupPage> _pages;
        [SerializeField] private PlayerPopupPageButton _pageButtonPrefab;
        [SerializeField] private Transform _pageButtonsContainer;
        private readonly List<PlayerPopupPageButton> _pageButtons = new();
        private int _pageIndex;

        protected override void Initialize()
        {
            for (var index = 0; index < _pages.Count; index++)
            {
                var page = _pages[index];
                page.Initialize();
                page.gameObject.SetActive(index == 0);
                var pageButton = Instantiate(_pageButtonPrefab, _pageButtonsContainer);
                _pageButtons.Add(pageButton);
                pageButton.Init(page);
                pageButton.SetInteractable(index != 0);
                pageButton.ButtonObservable.Subscribe(OnClick).AddTo(this);
            }

        }

        private void OnClick(PlayerPopupPage page)
        {
            _pages[_pageIndex].gameObject.SetActive(false);
            _pageButtons[_pageIndex].SetInteractable(true);
            _pageIndex = _pages.IndexOf(page);
            _pageButtons[_pageIndex].SetInteractable(false);
            _pages[_pageIndex].gameObject.SetActive(true);
        }
    }
}