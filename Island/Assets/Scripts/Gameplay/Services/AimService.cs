using Cysharp.Threading.Tasks;
using Island.Common.Services;
using Island.Gameplay.Configs.Aim;
using Island.Gameplay.Configs.World;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Inventory.Build;
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
        [Inject] private WorldConfig _worldConfig;
        [Inject] private InventoryService _inventoryService;
        [Inject] private InputService _inputService;
        
        private WorldObjectType _aimType;
        private GameObject _aimObject;
        private Ray _aimRay;
        private RaycastHit _hit;
        public IReadOnlyReactiveProperty<WorldObjectBase> TargetObject => _targetObject;
        private readonly IReactiveProperty<WorldObjectBase> _targetObject = new ReactiveProperty<WorldObjectBase>();
        
        public void OnNetworkInitialize()
        {
            Observable.EveryUpdate().Subscribe(OnTick).AddTo(CompositeDisposable);
        }

        private void SetTarget(WorldObjectBase targetObject)
        {
            _targetObject.Value = targetObject;

            if (_targetObject.Value is WorldGroundObject && _inventoryService.SelectedItem.ItemEntity is BuildItemEntity buildItemEntity)
            {
                ShowAimObject(buildItemEntity.ResultType);
            }
            else
            {
                HideAimObject();
            }
        }

        private void ShowAimObject(WorldObjectType type)
        {
            if (type > 0)
            {
                if (_aimType == type)
                {
                    _aimObject.transform.position = _hit.point;
                    return;
                }

                _aimType = type;
                _aimObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _aimObject.transform.position = _hit.point;
                return;
            }
            
            HideAimObject();
        }
        
        private void HideAimObject()
        {
            if (_aimObject != null)
            {
                Object.Destroy(_aimObject.gameObject);
                _aimType = WorldObjectType.Ground;
            }
        }

        private void OnTick(long frame)
        {
            OnUseButtonClicked(Time.deltaTime);
            HandleAim();
        }

        private void HandleAim()
        {
            _aimRay = Camera.main!.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(_aimRay, out _hit, _aimConfig.AimMaxDistance, _aimConfig.AimMask, QueryTriggerInteraction.Collide))
            {
                var worldItem = _hit.collider.GetComponentInParent<WorldObjectBase>();
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
                IPerformable performable = TargetObject.Value switch
                {
                    WorldCollectableObject or WorldCraftObject => TargetObject.Value,
                    _ => _inventoryService
                };

                performable.Perform(_inputService.PlayerActions.Interact.WasPressedThisFrame(), deltaTime).Forget();
            }
        }
    }
}