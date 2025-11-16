using System.Collections.Generic;
using Island.Common.Services;
using Island.Gameplay.Configs.Stats;
using Island.Gameplay.Panels.Inventory;
using Island.Gameplay.Services.DateTime;
using Island.Gameplay.Services.HUD;
using Island.Gameplay.Services.Server;
using TendedTarsier.Core.Panels;
using UniRx;
using TMPro;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Panels.HUD
{
    public class HUDPanel : PanelBase
    {
        [field: SerializeField] public TMP_Text DateTimeText { get; set; }
        [field: SerializeField] public TMP_Text JoinCodeText { get; set; }
        [field: SerializeField] public InfoTitleView InfoTitle { get; set; }
        [field: SerializeField] public InventoryCellView SelectedItem { get; set; }
        [field: SerializeField] public Transform StatsBarContainer { get; set; }
        [field: SerializeField] public ProgressBar ProgressBar { get; set; }

        [Inject] private DateTimeService _dateTimeService;
        [Inject] private ServerService _serverService;
        [Inject] private StatsConfig _statsConfig;

        private readonly Dictionary<StatType, StatBarView> _statBarViews = new();

        public void OnNetworkInitialize()
        {
            JoinCodeText.color = _serverService.IsServer ? Color.red : Color.green;
            JoinCodeText.SetText(_serverService.JoinCode);
            
            _dateTimeService.Minutes.Subscribe(OnDateTimeChanged).AddTo(this);
        }

        private void OnDateTimeChanged(float minutes)
        {
            DateTimeText.SetText(_dateTimeService.GetDateTime(minutes).ToString("yyyy-MM-dd HH:mm:ss"));
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