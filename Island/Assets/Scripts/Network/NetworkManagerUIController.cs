using UniRx;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Network
{
    public class NetworkManagerUIController : MonoBehaviour
    {
        [SerializeField] private Button _serverButton;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _clientButton;

        private void Awake()
        {
            _serverButton.OnClickAsObservable().Subscribe(_ => NetworkManager.Singleton.StartServer()).AddTo(this);
            _hostButton.OnClickAsObservable().Subscribe(_ => NetworkManager.Singleton.StartHost()).AddTo(this);
            _clientButton.OnClickAsObservable().Subscribe(_ => NetworkManager.Singleton.StartClient()).AddTo(this);
        }
    }
}
