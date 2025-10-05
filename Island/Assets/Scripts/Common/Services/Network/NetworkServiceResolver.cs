using Cysharp.Threading.Tasks;
using TendedTarsier.Core.Services;
using Zenject;

namespace Island.Common.Services
{
    public class NetworkServiceResolver : ServiceBase, IInitializable
    {
        private readonly NetworkService _networkService;
        private readonly DiContainer _container;

        public NetworkServiceResolver(NetworkService networkService, DiContainer container)
        {
            _networkService = networkService;
            _container = container;
        }

        public void Initialize()
        {
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            await UniTask.WaitUntil(() => _networkService.IsReady);
            var network = _container.ResolveAll<INetworkInitialize>();
            network.ForEach(t => t.OnNetworkInitialize());
        }
    }
}