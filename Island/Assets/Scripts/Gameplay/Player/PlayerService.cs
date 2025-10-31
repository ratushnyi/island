using Island.Common.Services;
using Island.Gameplay.Profiles;
using Island.Gameplay.Settings;
using TendedTarsier.Core.Services;
using Zenject;

namespace Island.Gameplay.Player
{
    public class PlayerService : ServiceBase, INetworkInitialize
    {
        [Inject] private readonly PlayerProfile _playerProfile;
        [Inject] private readonly PlayerConfig _playerConfig;
        [Inject] private readonly NetworkService _networkService;
        private PlayerController _playerController;

        public void Register(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public void OnNetworkInitialize()
        {
            _playerController.transform.position = _playerProfile.Position;
            _playerController.transform.rotation = _playerProfile.Rotation;
        }
    }
}