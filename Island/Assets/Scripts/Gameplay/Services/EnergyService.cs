using Island.Gameplay.Services.Stats;
using Island.Gameplay.Settings;
using JetBrains.Annotations;
using TendedTarsier.Core.Services;
using UniRx;
using Zenject;

namespace Island.Gameplay.Services
{
    [UsedImplicitly]
    public class EnergyService : ServiceBase
    {
        private PlayerConfig _playerConfig;
        private StatsService _statsService;

        private float _sprintDeposit;
        public readonly ReactiveProperty<bool> IsSprintPerformed = new();

        [Inject]
        private void Construct(PlayerConfig playerConfig, StatsService statsService)
        {
            _playerConfig = playerConfig;
            _statsService = statsService;
        }

        public bool TrackSprint(float duration)
        {
            var result = false;
            if (_sprintDeposit > duration)
            {
                _sprintDeposit -= duration;
                result = true;
            }
            else if (_statsService.TryApplyValue(_playerConfig.SprintFee, true))
            {
                _sprintDeposit = _playerConfig.SprintFee.Rate;
                result = true;
            }

            return result;
        }
    }
}