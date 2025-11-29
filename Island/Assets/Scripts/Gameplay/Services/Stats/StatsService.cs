using System;
using System.Collections.Generic;
using Island.Common.Services;
using Island.Gameplay.Configs.Stats;
using Island.Gameplay.Profiles.Stats;
using Island.Gameplay.Services.HUD;
using Island.Gameplay.Services.Server;
using JetBrains.Annotations;
using TendedTarsier.Core.Services;
using UniRx;
using Zenject;

namespace Island.Gameplay.Services.Stats
{
    [UsedImplicitly]
    public class StatsService : ServiceBase, IServerInitialize
    {
        private readonly Dictionary<StatType, IDisposable> _feeDisposables = new();

        private HUDService _hudService;
        private StatsConfig _statsConfig;
        private StatsProfile _statsProfile;

        [Inject]
        private void Construct(
            HUDService hudService,
            StatsConfig statsConfig,
            StatsProfile statsProfile)
        {
            _hudService = hudService;
            _statsConfig = statsConfig;
            _statsProfile = statsProfile;
        }

        public void OnNetworkInitialize()
        {
            InitializeProfile();
            InitializeStats();
            InitializeFees();
        }

        private void InitializeProfile()
        {
            foreach (var statEntity in _statsConfig.StatsList)
            {
                if (!_statsProfile.StatsDictionary.ContainsKey(statEntity.StatType))
                {
                    var levelEntity = statEntity[0];
                    _statsProfile.StatsDictionary.Add(statEntity.StatType, new StatProfileElement
                    {
                        Value = new ReactiveProperty<int>(levelEntity.DefaultValue),
                        Range = new ReactiveProperty<int>(levelEntity.MaxValue)
                    });
                }
            }
        }

        private void InitializeStats()
        {
            void onExperienceValueChanged(StatType statType, StatProfileElement statProfileElement)
            {
                var statsModel = _statsConfig[statType];

                var extraExperience = statProfileElement.Experience.Value - statsModel[statProfileElement.Level.Value].NextLevelExperience;
                if (extraExperience >= 0)
                {
                    statProfileElement.Experience.Value = extraExperience;
                    statProfileElement.Level.Value++;

                    var levelEntity = statsModel[statProfileElement.Level.Value];
                    statProfileElement.Value.Value = levelEntity.DefaultValue;
                    statProfileElement.Range.Value = levelEntity.MaxValue;
                }
            }

            void observeStat(StatType statType)
            {
                var profileElement = _statsProfile.StatsDictionary[statType];
                var levelModel = _statsConfig[statType][profileElement.Level.Value];

                StartApplyValueAutoApplyValue(statType, levelModel.RecoveryRate, levelModel.RecoveryValue);
            }

            foreach (var stat in _statsProfile.StatsDictionary)
            {
                var statsModel = _statsConfig[stat.Key];

                stat.Value.Experience
                    .Subscribe(_ => onExperienceValueChanged(stat.Key, stat.Value))
                    .AddTo(CompositeDisposable);
                
                observeStat(stat.Key);

                if (statsModel.StatBar)
                {
                    _hudService.ShowStatBar(stat.Key, statsModel, stat.Value);
                }
            }
        }

        private void InitializeFees()
        {
            foreach (var stat in _statsConfig.StatsFeeConditionalList)
            {
                var profileElement = _statsProfile.StatsDictionary[stat.Type];
                profileElement.Value.Subscribe(t => onFeeStatChanged(stat, profileElement, t)).AddTo(CompositeDisposable);
            }

            void onFeeStatChanged(StatFeeConditionModel condition, StatProfileElement statProfileElement, int currentValue)
            {
                switch (condition.Condition)
                {
                    case StatFeeConditionModel.FeeConditionType.MaxValue:
                        var levelModel = _statsConfig[condition.Type][statProfileElement.Level.Value];
                        if (currentValue == levelModel.MaxValue)
                        {
                            var disposable = StartApplyValueAutoApplyValue(condition.FeeModel.Type, condition.FeeModel.Rate, condition.FeeModel.Value);

                            _feeDisposables.Add(condition.Type, disposable);
                        }
                        else
                        {
                            if (_feeDisposables.TryGetValue(condition.Type, out var disposable))
                            {
                                disposable.Dispose();
                                _feeDisposables.Remove(condition.Type);
                            }
                        }
                        break;
                    case StatFeeConditionModel.FeeConditionType.MinValue:
                        if (currentValue == 0)
                        {
                            var disposable = StartApplyValueAutoApplyValue(condition.FeeModel.Type, condition.FeeModel.Rate, condition.FeeModel.Value);

                            _feeDisposables.Add(condition.Type, disposable);
                        }
                        else
                        {
                            if (_feeDisposables.TryGetValue(condition.Type, out var disposable))
                            {
                                disposable.Dispose();
                                _feeDisposables.Remove(condition.Type);
                            }
                        }
                        break;
                }
            }
        }

        public bool IsSuitable(StatFeeModel feeModel)
        {
            return IsSuitable(feeModel.Type, feeModel.Value);
        }

        public bool IsSuitable(StatType statType, int value)
        {
            if (value == 0)
            {
                return true;
            }

            var profileElement = _statsProfile.StatsDictionary[statType];
            var levelModel = _statsConfig[statType][profileElement.Level.Value];
            var hypotheticalValue = profileElement.Value.Value + value;
            var newValue = Math.Min(levelModel.MaxValue, profileElement.Value.Value + value);
            newValue = Math.Max(0, newValue);

            return hypotheticalValue == newValue;
        }

        public bool TryApplyValue(StatFeeModel feeModel, bool onlyFull = false)
        {
            return TryApplyValue(feeModel.Type, feeModel.Value, onlyFull);
        }

        public bool TryApplyValue(StatType statType, int value, bool onlyFull = false)
        {
            if (statType == StatType.None)
            {
                return false;
            }
            
            if (value == 0)
            {
                return true;
            }

            var profileElement = _statsProfile.StatsDictionary[statType];
            var levelModel = _statsConfig[statType][profileElement.Level.Value];
            if (onlyFull)
            {
                if (value > 0 && profileElement.Value.Value + value > levelModel.MaxValue)
                {
                    return false;
                }
                if (value < 0 && profileElement.Value.Value + value < 0)
                {
                    return false;
                }
            }
            var newValue = Math.Min(levelModel.MaxValue, profileElement.Value.Value + value);
            newValue = Math.Max(0, newValue);

            if (profileElement.Value.Value == newValue)
            {
                return false;
            }

            var experience = profileElement.Value.Value - newValue;
            profileElement.Value.Value = newValue;
            profileElement.Experience.Value += Math.Max(experience, 0);

            return true;
        }

        private IDisposable StartApplyValueAutoApplyValue(StatType type, int rate, int value)
        {
            return Observable.Timer(TimeSpan.FromSeconds(rate)).Repeat()
                .Subscribe(_ => TryApplyValue(type, value))
                .AddTo(CompositeDisposable);
        }
        
        public bool TrackFee(StatFeeModel feeModel, float duration)
        {
            var result = false;
            if (feeModel.Deposit > duration)
            {
                feeModel.Deposit -= duration;
                result = true;
            }
            else if (TryApplyValue(feeModel, true))
            {
                feeModel.Deposit = feeModel.Rate;
                result = true;
            }

            return result;
        }
    }
}