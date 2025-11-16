using Unity.Netcode;

namespace Island.Gameplay.Services.DateTime
{
    public class DateTimeServiceFacade : NetworkBehaviour
    {
        public NetworkVariable<float> Minutes = new();
    }
}