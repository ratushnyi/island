using Cysharp.Threading.Tasks;
using Island.Common.Services;
using Island.Common.Services.Network;
using Island.Gameplay.Configs.Aim;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Inventory.Build;
using Island.Gameplay.Services.World;
using Island.Gameplay.Services.World.Objects;
using JetBrains.Annotations;
using TendedTarsier.Core.Services;
using TendedTarsier.Core.Services.Input;
using UniRx;
using Unity.Netcode;
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
        [Inject] private WorldService _worldService;
        [Inject] private NetworkService _networkService;

        private WorldObjectBase _aimObject;
        private WorldObjectType _aimType;
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
            if (_targetObject.Value is WorldGroundObject && _inventoryService.SelectedItem.ItemEntity is BuildItemEntity buildItemEntity)
            {
                ShowAimObject(buildItemEntity.ResultType);
            }
            else if (_aimObject != null)
            {
                HideAimObject();
            }

            _targetObject.Value = targetObject;
        }

        private void ShowAimObject(WorldObjectType type)
        {
            if (type > 0)
            {
                if (_aimType == type)
                {
                    if (_aimObject != null)
                    {
                        _aimObject.transform.position = _hit.point;
                    }

                    return;
                }

                _aimType = type;
                _networkService.OnWorldObjectSpawned.First().Subscribe(OnWorldObjectSpawned);
                _networkService.Spawn(new NetworkSpawnRequest(0, type, _hit.point, owner: NetworkManager.Singleton.LocalClientId, notifyOwner: true));
                return;
            }

            HideAimObject();
        }

        private void OnWorldObjectSpawned(WorldObjectBase worldObject)
        {
            _aimObject = worldObject;

            foreach (var aimObjectCollider in _aimObject.Colliders)
            {
                aimObjectCollider.enabled = false;
            }
        }

        private void HideAimObject()
        {
            if (_aimObject != null)
            {
                _aimObject.Despawn_ServerRpc();
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
            if (Camera.main == null)
            {
                return;
            }
            
            _aimRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

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