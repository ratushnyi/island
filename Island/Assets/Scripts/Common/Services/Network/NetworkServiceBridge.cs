using Cysharp.Threading.Tasks;
using TendedTarsier.Core.Services;
using Zenject;

namespace Island.Common.Services
{
    public class NetworkServiceBridge : ServiceBase, IInitializable
    {
        private readonly NetworkServiceFacade _networkServiceFacade;
        private readonly NetworkService _networkService;
        private readonly DiContainer _container;

        public NetworkServiceBridge(NetworkServiceFacade networkServiceFacade, NetworkService networkService, DiContainer container)
        {
            _networkServiceFacade = networkServiceFacade;
            _networkService = networkService;
            _container = container;
        }

        public void Initialize()
        {
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            _networkService.Initialize(_networkServiceFacade);
            await UniTask.WaitUntil(() => _networkService.IsReady);
            _container.ResolveAll<INetworkPreInitialize>().ForEach(t => t.OnNetworkPreInitialize());
            _container.ResolveAll<INetworkInitialize>().ForEach(t => t.OnNetworkInitialize());
        }
    }
}