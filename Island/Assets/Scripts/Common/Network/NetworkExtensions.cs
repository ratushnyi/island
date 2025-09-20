using System;
using UniRx;
using Unity.Netcode;

namespace Island.Common.Network
{
    public static class NetworkExtensions
    {
        public static IObservable<ConnectionEventData> OnConnectionEventData(this NetworkManager networkManager)
        {
            return Observable.FromEvent<Action<NetworkManager, ConnectionEventData>, ConnectionEventData>(
                convert => (_, d) => convert(d),
                h => networkManager.OnConnectionEvent += h,
                h => networkManager.OnConnectionEvent -= h
            );
        }
    }
}