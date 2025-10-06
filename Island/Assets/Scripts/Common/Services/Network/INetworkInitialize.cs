namespace Island.Common.Services
{
    public interface INetworkInitialize
    {
        public void OnNetworkInitialize();
    }    
    
    public interface INetworkPreInitialize
    {
        public void OnNetworkPreInitialize();
    }
}