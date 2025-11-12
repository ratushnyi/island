using System;
using Island.Common.Services;
using Island.Gameplay.Configs.DateTime;
using Island.Gameplay.Profiles;
using JetBrains.Annotations;
using TendedTarsier.Core.Services;
using UniRx;
using Zenject;

namespace Island.Gameplay.Services.DateTime
{
    [UsedImplicitly]
    public class DateTimeService : ServiceBase, INetworkInitialize
    {
        [Inject] private NetworkService _networkService;
        [Inject] private DateTimeProfile _profile;
        [Inject] private DateTimeConfig _config;
        
        public void OnNetworkInitialize()
        {
            if (!_networkService.IsServer)
            {
                return;
            }
            
            Observable.Timer(TimeSpan.FromSeconds(_config.Rate)).Repeat()
                .Subscribe(_ => _profile.Minutes.Value++)
                .AddTo(CompositeDisposable);
        }
    }
}