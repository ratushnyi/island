using Cysharp.Threading.Tasks;
using TendedTarsier.Core.Services.Modules;

namespace Island.Gameplay.Module
{
    public class GameplayIslandModuleController : ModuleControllerBase
    {
        public override async UniTask Initialize()
        {
            await UniTask.Delay(1000);
        }
    }
}