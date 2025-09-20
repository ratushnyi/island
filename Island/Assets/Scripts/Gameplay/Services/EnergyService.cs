using System;
using Island.Gameplay.Profiles;
using Island.Gameplay.Settings;
using TendedTarsier.Core.Services;
using UniRx;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services
{
    public class EnergyService : ServiceBase, IInitializable
    {
        private PlayerProfile _playerProfile;
        private PlayerConfig _playerConfig;
        
        private float _sprintDeposit;

        [Inject]
        private void Construct(PlayerProfile playerProfile, PlayerConfig playerConfig)
        {
            _playerProfile = playerProfile;
            _playerConfig = playerConfig;
        }

        public void Initialize()
        {
            Observable.Timer(TimeSpan.FromSeconds(1)).Repeat().Subscribe(OnTick).AddTo(CompositeDisposable);
        }

        private void OnTick(long deltaTime)
        {
            Debug.Log($"Energy: {_playerProfile.Energy}");
            _playerProfile.Energy = Mathf.Min(_playerProfile.Energy + 1, 100);
        }

        public bool TrackSprint(float duration)
        {
            var result = false;
            if(_sprintDeposit > duration)
            {
                _sprintDeposit -= duration;
                result = true;
            }
            else if(_playerProfile.Energy > _playerConfig.SprintCost)
            {
                _sprintDeposit = 1;
                _playerProfile.Energy-=10;
                result = true;
            }
            
            Debug.Log($"Energy: {_playerProfile.Energy}");
            
            return result;
        }
    }
}