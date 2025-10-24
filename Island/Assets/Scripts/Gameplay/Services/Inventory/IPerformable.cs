using Cysharp.Threading.Tasks;

namespace Island.Gameplay.Services.Inventory
{
    public interface IPerformable
    {
        UniTask<bool> Perform(bool isJustUsed, float deltaTime);
    }
}