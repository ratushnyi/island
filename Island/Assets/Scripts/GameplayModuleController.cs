using Cysharp.Threading.Tasks;
using TendedTarsier.Core.Services.Modules;

public class GameplayModuleController : ModuleControllerBase
{
    public override async UniTask Initialize()
    {
        await UniTask.Delay(1000);
    }
}