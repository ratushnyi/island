using Cysharp.Threading.Tasks;

namespace Island.Gameplay.Services.World.Objects
{
    public class WorldGroundObject : WorldObjectBase
    {
        public override string Name => Type.ToString();

        public override UniTask<bool> Perform(bool isJustUsed, float deltaTime)
        {
            return UniTask.FromResult(false);
        }
    }
}