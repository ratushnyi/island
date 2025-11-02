using Island.Gameplay.Services.World.Objects;
using JetBrains.Annotations;
using TendedTarsier.Core.Services;
using UniRx;

namespace Island.Gameplay.Services
{
    [UsedImplicitly]
    public class AimService : ServiceBase
    {
        public IReadOnlyReactiveProperty<WorldObjectBase> TargetObject => _targetObject;
        private readonly IReactiveProperty<WorldObjectBase> _targetObject = new ReactiveProperty<WorldObjectBase>();

        public void SetTarget(WorldObjectBase targetObject)
        {
            _targetObject.Value = targetObject;
        }
    }
}