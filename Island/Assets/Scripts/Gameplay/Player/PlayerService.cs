using Island.Common.Services;
using Island.Gameplay.Profiles;
using Island.Gameplay.Services.Server;
using Island.Gameplay.Settings;
using TendedTarsier.Core.Services;
using UniRx;
using Zenject;

namespace Island.Gameplay.Player
{
    public class PlayerService : ServiceBase, IServerInitialize
    {
        [Inject] private readonly PlayerProfile _playerProfile;
        [Inject] private readonly PlayerConfig _playerConfig;
        [Inject] private readonly ServerService _serverService;
        private PlayerController _playerController;

        public void Register(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public void OnNetworkInitialize()
        {
            _playerController.NetworkObject.NetworkTransforms[0].Teleport(_playerProfile.Position, _playerProfile.Rotation, _playerController.transform.localScale);
            _playerController.OnPositionChanged.Subscribe(t => _playerProfile.Position = t).AddTo(CompositeDisposable);
            _playerController.OnRotationChanged.Subscribe(t => _playerProfile.Rotation = t).AddTo(CompositeDisposable);
        }
    }
}