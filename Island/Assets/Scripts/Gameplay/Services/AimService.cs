using Cysharp.Threading.Tasks;
using Island.Common.Services;
using Island.Gameplay.Configs.Aim;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.World.Objects;
using JetBrains.Annotations;
using TendedTarsier.Core.Services;
using TendedTarsier.Core.Services.Input;
using UniRx;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services
{
    [UsedImplicitly]
    public class AimService : ServiceBase, INetworkInitialize
    {
        [Inject] private AimConfig _aimConfig;
        [Inject] private InventoryService _inventoryService;
        [Inject] private InputService _inputService;
        
        private Ray _aimRay;
        public IReadOnlyReactiveProperty<WorldObjectBase> TargetObject => _targetObject;
        private readonly IReactiveProperty<WorldObjectBase> _targetObject = new ReactiveProperty<WorldObjectBase>();
        
        public void OnNetworkInitialize()
        {
            Observable.EveryUpdate().Subscribe(OnTick).AddTo(CompositeDisposable);
        }

        private void SetTarget(WorldObjectBase targetObject)
        {
            _targetObject.Value = targetObject;
        }

        private void OnTick(long frame)
        {
            OnUseButtonClicked(Time.deltaTime);
            HandleAim();
        }

        private void HandleAim()
        {
            _aimRay = Camera.main!.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(_aimRay, out var hit, _aimConfig.AimMaxDistance, _aimConfig.AimMask, QueryTriggerInteraction.Collide))
            {
                var worldItem = hit.collider.GetComponentInParent<WorldObjectBase>();
                if (worldItem != null)
                {
                    SetTarget(worldItem);
                    return;
                }
            }
            

            SetTarget(null);
        }

        private void OnUseButtonClicked(float deltaTime)
        {
            if (_inputService.PlayerActions.Interact.inProgress)
            {
                if (TargetObject.Value is WorldCollectableObject collectableObject)
                {
                    collectableObject.Perform(_inputService.PlayerActions.Interact.WasPressedThisFrame()).Forget();
                    return;
                }

                if (TargetObject.Value is WorldCraftObject transformerObject)
                {
                    transformerObject.Perform(_inputService.PlayerActions.Interact.WasPressedThisFrame()).Forget();
                    return;
                }

                _inventoryService.PerformSelectedItem(_inputService.PlayerActions.Interact.WasPressedThisFrame(), deltaTime).Forget();
            }
        }
    }
}