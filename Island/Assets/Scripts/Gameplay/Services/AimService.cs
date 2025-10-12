using Island.Gameplay.Services.World.Items;
using JetBrains.Annotations;
using TendedTarsier.Core.Services;
using UniRx;

namespace Island.Gameplay.Services
{
    [UsedImplicitly]
    public class AimService : ServiceBase
    {
        public IReadOnlyReactiveProperty<WorldItemObject> TargetObject => _targetObject;
        private readonly IReactiveProperty<WorldItemObject> _targetObject = new ReactiveProperty<WorldItemObject>();

        public void SetTarget(WorldItemObject targetObject)
        {
            _targetObject.Value = targetObject;
        }
    }
}