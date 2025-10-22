using System.Collections.Generic;
using Island.Common.Services;
using Island.Gameplay.Configs.Stats;
using Island.Gameplay.Panels.Inventory;
using Island.Gameplay.Services.HUD;
using TendedTarsier.Core.Panels;
using TMPro;
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
        [field: SerializeField] public ProgressBar ProgressBar { get; set; }

        private NetworkService _networkService;
        private StatsConfig _statsConfig;

        private readonly Dictionary<StatType, StatBarView> _statBarViews = new();

        [Inject]
        private void Construct(NetworkService networkService, StatsConfig statsConfig)
        {
            _networkService = networkService;
            _statsConfig = statsConfig;
        }

        public void OnNetworkInitialize()
        {
            if (_networkService.IsServer)
            {
                JoinCodeText.color = Color.red;
                JoinCodeText.SetText(_networkService.JoinCode);
            }
            else
            {
                JoinCodeText.color = Color.green;
                JoinCodeText.SetText(_networkService.ServerId);
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