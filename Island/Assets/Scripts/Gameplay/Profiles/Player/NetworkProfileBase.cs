using Island.Common.Services;
using Island.Gameplay.Services.Server;
using TendedTarsier.Core.Services.Profile;
using Zenject;

namespace Island.Gameplay.Profiles
{
    public abstract class ServerProfileBase : ProfileBase, IServerPreInitialize
    {
        [Inject] private ServerService _serverService;
        protected abstract string NetworkName { get; }
        public override string Name => $"{_serverService.ServerId}_{NetworkName}";

        protected override void RegisterProfile()
        {
            // Do not register the network profile
        }

        public void OnNetworkPreInitialize()
        {
            base.RegisterProfile();
        }
    }
}