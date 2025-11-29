using Cysharp.Threading.Tasks;
using Island.Common;
using Island.Common.Services;
using Island.Common.Services.Network;
using Island.Gameplay.Configs.Aim;
using Island.Gameplay.Services.Build;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Inventory.Build;
using Island.Gameplay.Services.Server;
using Island.Gameplay.Services.World.Objects;
using JetBrains.Annotations;
using TendedTarsier.Core.Panels;
using TendedTarsier.Core.Services;
using TendedTarsier.Core.Services.Input;
using TendedTarsier.Core.Utilities.Extensions;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using ServerSpawnRequest = Island.Gameplay.Services.Server.ServerSpawnRequest;

namespace Island.Gameplay.Services
{
    [UsedImplicitly]
    public class AimService : ServiceBase, IServerInitialize
    {
        [Inject] private AimConfig _aimConfig;
        [Inject] private InventoryService _inventoryService;
        [Inject] private InputService _inputService;
        [Inject] private ServerService _serverService;
        [Inject] private BuildService _buildService;
        [Inject] private PanelService _panelService;

        private WorldObjectType _aimType;
        private Ray _aimRay;
        private RaycastHit _hit;
        private WorldObjectBase _aimObject;
        public WorldObjectBase AimObject => _aimObject;
        public IReadOnlyReactiveProperty<WorldObjectBase> TargetObject => _targetObject;
        private readonly IReactiveProperty<WorldObjectBase> _targetObject = new ReactiveProperty<WorldObjectBase>();

        public void OnNetworkInitialize()
        {
            Observable.EveryUpdate().Subscribe(OnTick).AddTo(CompositeDisposable);
        }

        private void SetTarget(WorldObjectBase targetObject)
        {
            if (_inventoryService.SelectedItem.ItemEntity is BuildItemEntity buildItemEntity)
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
                        _aimObject.NetworkObject.NetworkTransforms[0].Teleport(_hit.point, _aimObject.transform.rotation, _aimObject.transform.localScale);
                        _aimObject.SetColor_ServerRpc(_buildService.IsSuitablePlace(_aimType) ? _aimConfig.AimObjectSuitableColor : _aimConfig.AimObjectUnsuitableColor);
                    }

                    return;
                }

                _aimType = type;
                _serverService.OnWorldObjectSpawned.First().Subscribe(OnWorldObjectSpawned);
                _serverService.Spawn(new ServerSpawnRequest(IslandExtensions.AimHash, type, _hit.point, owner: NetworkManager.Singleton.LocalClientId, notifyOwner: true), false);
                return;
            }

            HideAimObject();
        }

        private void OnWorldObjectSpawned(WorldObjectBase worldObject)
        {
            _aimObject = worldObject;
            _aimObject.gameObject.SetLayerRecursively(_aimConfig.AimObjectLayer);
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

            _aimRay = Camera.main.ViewportPointToRay(_aimConfig.AimPosition);

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
                IPerformable performable = (TargetObject.Value is ISelfPerformable, _panelService.IsAnyPopupOpen) switch
                {
                    (true, false) => TargetObject.Value,
                    _ => _inventoryService
                };

                performable.Perform(_inputService.PlayerActions.Interact.WasPressedThisFrame(), deltaTime).Forget();
            }
        }
    }
}