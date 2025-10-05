using System.Collections.Generic;
using Island.Common.Services;
using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Configs.Stats;
using Island.Gameplay.Panels.Inventory;
using Island.Gameplay.Profiles.Inventory;
using TendedTarsier.Core.Panels;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Panels.HUD
{
    public class HUDPanel : PanelBase
    {
        [field: SerializeField] public TMP_Text JoinCodeText { get; set; }
        [field: SerializeField] public InfoTitleView InfoTitle { get; set; }
        [field: SerializeField] public InventoryCellView SelectedItem { get; set; }
        [field: SerializeField] public Transform StatsBarContainer { get; set; }

        private NetworkService _networkService;
        private InventoryProfile _inventoryProfile;
        private InventoryConfig _inventoryConfig;
        private StatsConfig _statsConfig;

        private readonly Dictionary<StatType, StatBarView> _statBarViews = new();

        [Inject]
        private void Construct(
            NetworkService networkService,
            InventoryProfile inventoryProfile,
            InventoryConfig inventoryConfig,
            StatsConfig statsConfig)
        {
            _networkService = networkService;
            _statsConfig = statsConfig;
            _inventoryConfig = inventoryConfig;
            _inventoryProfile = inventoryProfile;
        }

        protected override void Initialize()
        {
            _inventoryProfile.SelectedItem.Subscribe(itemId =>
            {
                if (string.IsNullOrEmpty(itemId))
                {
                    SelectedItem.SetEmpty();
                    return;
                }

                SelectedItem.SetItem(_inventoryConfig[itemId], _inventoryProfile.InventoryItems[itemId]);
            }).AddTo(CompositeDisposable);
        }

        public void OnNetworkInitialize()
        {
            if (_networkService.IsServer)
            {
                JoinCodeText.SetText(_networkService.JoinCode);
            }
            else
            {
                JoinCodeText.gameObject.SetActive(false);
            }
        }

        public InfoTitleView GetInfoTitle()
        {
            return InfoTitle;
        }

        public StatBarView GetStatBar(StatType statType)
        {
            if (_statBarViews.TryGetValue(statType, out var bar))
            {
                return bar;
            }

            var newStatBar = Instantiate(_statsConfig.StatBarView, StatsBarContainer);
            _statBarViews[statType] = newStatBar;
            return newStatBar;
        }
    }
}