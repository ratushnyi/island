using Cysharp.Threading.Tasks;

namespace Island.Gameplay.Services.World.Items
{
    public class WorldGroundObject : WorldObjectBase
    {
        public override string Name => Type.ToString();

        public override UniTask<bool> Perform(bool isJustUsed)
        {
            return UniTask.FromResult(false);
        }
    }
}