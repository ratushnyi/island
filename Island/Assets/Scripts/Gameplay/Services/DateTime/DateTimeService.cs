using Island.Common.Services;
using Island.Common.Services.Network;
using Island.Gameplay.Configs.DateTime;
using Island.Gameplay.Profiles;
using Island.Gameplay.Services.Server;
using JetBrains.Annotations;
using TendedTarsier.Core.Utilities.Extensions;
using UniRx;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.DateTime
{
    [UsedImplicitly]
    public class DateTimeService : NetworkServiceBase<DateTimeServiceFacade>, IServerInitialize
    {
        [Inject] private ServerService _serverService;
        [Inject] private DateTimeProfile _profile;
        [Inject] private DateTimeConfig _config;

        public IReadOnlyReactiveProperty<float> Minutes => Facade.Minutes.AsReactiveProperty();
        
        public void OnNetworkInitialize()
        {
            if (!_serverService.IsServer)
            {
                return;
            }

            Observable.EveryUpdate().Subscribe(Update).AddTo(Facade);
        }

        private void Update(long tick)
        {
            var deltaTime = Time.deltaTime;
            var secondsPerMinute = _config.DayDuration / 1440f;
            var minutesToAdd = deltaTime / secondsPerMinute;
            _profile.Minutes.Value += minutesToAdd;
            Facade.Minutes.Value = _profile.Minutes.Value;
        }

        public System.DateTime GetDateTime(float minutes)
        {
            var countOfDays = System.DateTime.DaysInMonth(_config.StartYear, _config.StartMonth);
            var day = _config.StartDate;
            if (day > countOfDays)
            {
                Debug.LogWarning($"Start day is greater than the number of days in the month. Adjusting {nameof(DateTimeConfig)} to last day of the month.");
                day = _config.StartDate;
            }

            System.DateTime startDate = new System.DateTime(_config.StartYear, _config.StartMonth, day);
            System.DateTime currentDate = startDate.AddMinutes(minutes);
            return currentDate;
        }
    }
}