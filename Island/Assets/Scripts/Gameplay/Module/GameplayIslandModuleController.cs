using Cysharp.Threading.Tasks;
using TendedTarsier.Core.Services.Modules;

namespace Island.Gameplay.Module
{
    public class GameplayIslandModuleController : ModuleControllerBase
    {
        public override UniTask Initialize()
        {
            return UniTask.Delay(1);
        }
    }
}