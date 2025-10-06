using UniRx;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Island.Common.Services
{
    public class NetworkServiceFacade : NetworkBehaviour
    {
        public string ServerId => _serverId.Value.Value;
        public readonly ReactiveProperty<bool> IsPaused = new();
        private readonly NetworkVariable<bool> _isPausedNetwork = new();
        private readonly NetworkVariable<FixedString32Bytes> _serverId = new();

        public override void OnNetworkSpawn()
        {
            _isPausedNetwork.OnValueChanged += OnPauseChanged;

            OnPauseChanged(false, _isPausedNetwork.Value);
        }

        public override void OnNetworkDespawn()
        {
            _isPausedNetwork.OnValueChanged -= OnPauseChanged;
            
        }

        public void SetServerId(string serverId)
        {
            if (!IsServer)
            {
                return;
            }

            _serverId.Value = new FixedString32Bytes(serverId);;
        }

        public void SetPaused(bool value)
        {
            if (!IsServer)
            {
                return;
            }

            _isPausedNetwork.Value = value;
        }
        

        private void OnPauseChanged(bool oldValue, bool newValue)
        {
            IsPaused.Value = newValue;
            Time.timeScale = newValue ? 0f : 1f;
        }
    }
}