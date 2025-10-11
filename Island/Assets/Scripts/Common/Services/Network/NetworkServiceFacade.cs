using UniRx;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Island.Common.Services
{
    public class NetworkServiceFacade : NetworkBehaviour
    {
        public readonly ReactiveProperty<bool> IsPaused = new();
        private readonly NetworkVariable<bool> _isPausedNetwork = new();

        public readonly ReactiveProperty<string> ServerId = new();
        private readonly NetworkVariable<FixedString32Bytes> _serverId = new();

        public override void OnNetworkSpawn()
        {
            _isPausedNetwork.OnValueChanged += OnPauseChanged;
            _serverId.OnValueChanged += OnServerIdChanged;

            OnPauseChanged(false, _isPausedNetwork.Value);
            OnServerIdChanged(string.Empty, _serverId.Value);
        }

        public override void OnNetworkDespawn()
        {
            _isPausedNetwork.OnValueChanged -= OnPauseChanged;
            _serverId.OnValueChanged -= OnServerIdChanged;
        }

        public void SetServerId(string serverId)
        {
            if (!IsServer)
            {
                return;
            }

            _serverId.Value = new FixedString32Bytes(serverId);
            
            ServerId.Value = _serverId.Value.Value;
        }

        public void SetPaused(bool value)
        {
            if (!IsServer)
            {
                return;
            }

            _isPausedNetwork.Value = value;
        }

        private void OnPauseChanged(bool _, bool newValue)
        {
            IsPaused.Value = newValue;
            Time.timeScale = newValue ? 0f : 1f;
        }

        private void OnServerIdChanged(FixedString32Bytes _, FixedString32Bytes newValue)
        {
            ServerId.Value = newValue.Value;
        }
    }
}