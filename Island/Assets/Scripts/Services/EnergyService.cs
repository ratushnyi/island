using System;
using TendedTarsier.Core.Services;
using UniRx;
using Zenject;
using Observable = UniRx.Observable;

namespace Module
{
    public class EnergyService : ServiceBase, IInitializable
    {
        private PlayerProfile _playerProfile;
        
        private float _counter;

        [Inject]
        private void Construct(PlayerProfile playerProfile)
        {
            _playerProfile = playerProfile;
        }

        public void Initialize()
        {
            Observable.EveryUpdate().Skip(TimeSpan.FromSeconds(1)).Subscribe(OnTick).AddTo(CompositeDisposable);
        }

        private void OnTick(long deltaTime)
        {
            _playerProfile.Energy += 1;
        }

        public void TrackSprint(float duration)
        {
            _counter += duration;

            if (_counter > 1)
            {
                _counter--;
                _playerProfile.Energy--;
            }
        }
    }
}