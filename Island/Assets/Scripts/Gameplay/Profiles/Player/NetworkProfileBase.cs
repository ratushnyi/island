using Island.Common.Services;
using TendedTarsier.Core.Services.Profile;
using Zenject;

namespace Island.Gameplay.Profiles
{
    public abstract class NetworkProfileBase : ProfileBase, INetworkPreInitialize
    {
        private NetworkService _networkService;
        protected abstract string NetworkName { get; }
        public override string Name => $"{_networkService.ServerId}_{NetworkName}";

        [Inject]
        public void Construct(NetworkService networkService)
        {
            _networkService = networkService;
        }

        protected override void RegisterProfile() { }

        public void OnNetworkPreInitialize()
        {
            base.RegisterProfile();
        }
    }
}