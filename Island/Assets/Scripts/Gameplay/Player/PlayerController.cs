using Cysharp.Threading.Tasks;
using Island.Common.Services;
using Island.Gameplay.Services;
using Island.Gameplay.Services.HUD;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Stats;
using Island.Gameplay.Services.World;
using Island.Gameplay.Services.World.Items;
using Island.Gameplay.Settings;
using TendedTarsier.Core.Panels;
using TendedTarsier.Core.Services.Input;
using UniRx;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using InputExtensions = TendedTarsier.Core.Utilities.Extensions.InputExtensions;

namespace Island.Gameplay.Player
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private Transform _head;
        [SerializeField] private Camera _camera;
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private LayerMask _aimMask;
        [SerializeField] private float _aimMaxDistance = 3;

        [Inject] private InputService _inputService;
        [Inject] private WorldService _worldService;
        [Inject] private AimService _aimService;
        [Inject] private StatsService _statService;
        [Inject] private SettingsService _settingsService;
        [Inject] private InventoryService _inventoryService;
        [Inject] private HUDService _hudService;
        [Inject] private PanelService _panelService;
        [Inject] private PlayerService _playerService;
        [Inject] private PlayerConfig _playerConfig;
        [Inject] private CameraConfig _cameraConfig;

        private Ray _aimRay;
        private float _cameraPitch;
        private float _verticalVelocity;
        private float _sprintLerp;
        private bool IsMoving => _characterController.velocity.magnitude > 0;
        private bool IsRunning => IsMoving && (_inputService.PlayerActions.Sprint.IsPressed() || SprintButtonToggleState);

        private bool SprintButtonToggleState
        {
            get => !InputExtensions.IsMouseKeyboardInput && _statService.IsSprintPerformed.Value;
            set => _statService.IsSprintPerformed.Value = value;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                return;
            }

            _playerService.Register(this);

            _cinemachineCamera.gameObject.SetActive(true);
            _cinemachineCamera.Lens.FieldOfView = _settingsService.Fov.Value;

            Observable.EveryUpdate().Subscribe(OnTick).AddTo(this);
            _settingsService.Fov.Subscribe(OnFovChanged).AddTo(this);
        }

        private void OnUseButtonClicked(float deltaTime)
        {
            if (_inputService.PlayerActions.Interact.inProgress)
            {
                if (_aimService.TargetObject.Value is WorldTransformerObject itemObject)
                {
                    itemObject.Perform(_inputService.PlayerActions.Interact.WasPressedThisFrame()).Forget();
                    return;
                }
                
                _inventoryService.PerformSelectedItem(_inputService.PlayerActions.Interact.WasPressedThisFrame(), deltaTime).Forget();
            }
        }

        private void OnFovChanged(int value)
        {
            _cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(value, value * _cameraConfig.FovSprintModifier, _sprintLerp);
        }

        private void OnTick(long frame)
        {
            OnUseButtonClicked(Time.deltaTime);
            HandleSprint(Time.deltaTime);
            HandleVerticalVelocity(Time.deltaTime);
            HandleMove(Time.deltaTime);
            HandleCamera();
            HandleAim();
        }

        private void HandleVerticalVelocity(float deltaTime)
        {
            if (_inputService.PlayerActions.Jump.WasPressedThisFrame() && _characterController.isGrounded)
            {
                _verticalVelocity = _playerConfig.JumpForce;
            }
            else if (!_characterController.isGrounded)
            {
                _verticalVelocity -= _playerConfig.Gravity * deltaTime;
            }
            else
            {
                _verticalVelocity = -_playerConfig.Gravity;
            }
        }

        private void HandleMove(float deltaTime)
        {
            if (_panelService.IsAnyPopupOpen)
            {
                return;
            }

            var movementSpeed = Mathf.Lerp(_playerConfig.WalkSpeed, _playerConfig.SprintSpeed, _sprintLerp);
            var moveInput = _inputService.PlayerActions.Move.ReadValue<Vector2>() * movementSpeed;
            var moveDirection = NetworkObject.transform.right * moveInput.x + NetworkObject.transform.forward * moveInput.y + Vector3.up * _verticalVelocity;
            moveDirection *= deltaTime;

            _characterController.Move(moveDirection);
        }

        private void HandleSprint(float deltaTime)
        {
            if (_panelService.IsAnyPopupOpen)
            {
                return;
            }

            if (_inputService.PlayerActions.Sprint.WasPressedThisFrame())
            {
                if (SprintButtonToggleState || !IsMoving)
                {
                    SprintButtonToggleState = false;
                    return;
                }

                _playerConfig.SprintFee.Deposit = 0;
                SprintButtonToggleState = true;
            }

            if (IsRunning && _statService.TrackFee(_playerConfig.SprintFee, deltaTime))
            {
                _sprintLerp = Mathf.Lerp(_sprintLerp, 1, deltaTime * _playerConfig.SprintGetSpeed);
            }
            else if (_sprintLerp > 0)
            {
                SprintButtonToggleState = false;
                _sprintLerp = Mathf.Lerp(_sprintLerp, 0, deltaTime * _playerConfig.SprintFallSpeed);
            }
        }

        private void HandleCamera()
        {
            if (_panelService.IsAnyPopupOpen)
            {
                return;
            }


            var lookInput = GetActiveCameraInput() * _settingsService.CameraSensitivity.Value / 100;

            NetworkObject.transform.Rotate(Vector3.up * lookInput.x);
            _cameraPitch -= lookInput.y;
            _cameraPitch = Mathf.Clamp(_cameraPitch, _cameraConfig.PitchLimits.x, _cameraConfig.PitchLimits.y);
            _head.transform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
            _cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(_settingsService.Fov.Value, _settingsService.Fov.Value * _cameraConfig.FovSprintModifier, _sprintLerp);
        }

        private Vector2 GetActiveCameraInput()
        {
            if (!Application.isMobilePlatform)
            {
                return _inputService.PlayerActions.Look.ReadValue<Vector2>();
            }

            if (_inputService.PlayerActions.Look.inProgress)
            {
                if (!InputExtensions.IsOverUI(_inputService.PlayerActions.Look.activeControl.device.deviceId))
                {
                    return _inputService.PlayerActions.Look.ReadValue<Vector2>();
                }
            }

            return default;
        }

        private void HandleAim()
        {
            _aimRay = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(_aimRay, out var hit, _aimMaxDistance, _aimMask, QueryTriggerInteraction.Collide))
            {
                var worldItem = hit.collider.GetComponentInParent<WorldObjectBase>();
                if (worldItem != null)
                {
                    _aimService.SetTarget(worldItem);
                    return;
                }
            }

            _aimService.SetTarget(null);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_aimRay.origin, _aimRay.origin + _aimRay.direction * _aimMaxDistance);
        }
    }
}