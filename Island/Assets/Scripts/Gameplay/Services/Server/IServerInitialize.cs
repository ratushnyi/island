namespace Island.Gameplay.Services.Server
{
    public interface IServerInitialize
    {
        public void OnNetworkInitialize();
    }    
    
    public interface IServerPreInitialize
    {
        public void OnNetworkPreInitialize();
    }
}