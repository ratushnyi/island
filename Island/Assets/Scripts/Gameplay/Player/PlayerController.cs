using System;
using Island.Common;
using Island.Common.Services;
using Island.Gameplay.Services;
using Island.Gameplay.Services.HUD;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.World;
using Island.Gameplay.Settings;
using TendedTarsier.Core.Services.Input;
using UniRx;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace Island.Gameplay.Player
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private CinemachineCamera _camera;
        [SerializeField] private CharacterController _characterController;

        private InputService _inputService;
        private WorldService _worldService;
        private EnergyService _energyService;
        private SettingsService _settingsService;
        private InventoryService _inventoryService;
        private HUDService _hudService;
        private PlayerConfig _playerConfig;
        private CameraConfig _cameraConfig;

        private float _cameraPitch;
        private float _verticalVelocity;
        private float _sprintLerp;
        private bool _sprintButtonToggleState;

        private WorldItemObject _targetObject;
        private IDisposable _targetObjectDispose;

        private bool IsMoving => _characterController.velocity.magnitude > 0;
        private bool IsRunning => IsMoving && (_inputService.PlayerActions.Sprint.IsPressed() || SprintButtonToggleState);
        private bool SprintButtonToggleState
        {
            get => (Application.isMobilePlatform || Application.isConsolePlatform || Application.isEditor) && _sprintButtonToggleState;
            set => _sprintButtonToggleState = value;
        }


        [Inject]
        private void Construct(InputService inputService, WorldService worldService, EnergyService energyService, SettingsService settingsService, InventoryService inventoryService, HUDService hudService, PlayerConfig playerConfig, CameraConfig cameraConfig)
        {
            _inputService = inputService;
            _worldService = worldService;
            _energyService = energyService;
            _settingsService = settingsService;
            _inventoryService = inventoryService;
            _hudService = hudService;
            _playerConfig = playerConfig;
            _cameraConfig = cameraConfig;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                return;
            }

            _camera.gameObject.SetActive(true);
            _camera.Lens.FieldOfView = _settingsService.Fov.Value;

            Observable.EveryUpdate().Subscribe(OnTick).AddTo(this);
            _inputService.OnAttackButtonStarted.Subscribe(OnAttackButtonClicked).AddTo(this);
            _settingsService.Fov.Subscribe(OnFovChanged).AddTo(this);
        }

        private void OnCurrentWorldObjectChanged(WorldItemObject item)
        {
            _targetObjectDispose?.Dispose();
            _targetObject = item;

            if (_targetObject != null)
            {
                _targetObjectDispose = _targetObject.OnObjectDespawn.Subscribe(OnCurrentWorldObjectChanged);
            }

            var itemName = _targetObject?.Name;
            _hudService.SetInfoTitle(itemName);
        }

        private void OnAttackButtonClicked(InputAction.CallbackContext _)
        {
            if (!IsOwner)
            {
                return;
            }

            if (_targetObject != null)
            {
                _targetObject.Destroy_ServerRpc();
            }
        }

        private void OnFovChanged(int value)
        {
            _camera.Lens.FieldOfView = Mathf.Lerp(value, value * _cameraConfig.FovSprintModifier, _sprintLerp);
        }

        private void OnTick(long frame)
        {
            HandleSprint(Time.deltaTime);
            HandleVerticalVelocity(Time.deltaTime);
            HandleMove(Time.deltaTime);
            HandleCamera();
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
            var movementSpeed = Mathf.Lerp(_playerConfig.WalkSpeed, _playerConfig.SprintSpeed, _sprintLerp);
            var moveInput = _inputService.PlayerActions.Move.ReadValue<Vector2>() * movementSpeed;
            var moveDirection = NetworkObject.transform.right * moveInput.x + NetworkObject.transform.forward * moveInput.y + Vector3.up * _verticalVelocity;
            moveDirection *= deltaTime;

            _characterController.Move(moveDirection);
        }

        private void HandleSprint(float deltaTime)
        {
            if (_inputService.PlayerActions.Sprint.WasPressedThisFrame())
            {
                if (SprintButtonToggleState || !IsMoving)
                {
                    SprintButtonToggleState = false;
                    return;
                }

                SprintButtonToggleState = true;
            }

            if (IsRunning && _energyService.TrackSprint(deltaTime))
            {
                _sprintLerp = Mathf.Lerp(_sprintLerp, 1, deltaTime * _playerConfig.SprintGetSpeed);
            }
            else if (_sprintLerp > 0)
            {
                SprintButtonToggleState = false;
                _sprintLerp = Mathf.Lerp(_sprintLerp, 0, deltaTime * _playerConfig.SprintFallSpeed);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsOwner)
            {
                return;
            }

            switch (other.tag)
            {
                case CommonConstants.ItemTag:
                    OnCurrentWorldObjectChanged(other.GetComponent<WorldItemObject>());
                    break;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsOwner)
            {
                return;
            }

            switch (other.tag)
            {
                case CommonConstants.ItemTag:
                    if (_targetObject == other.GetComponent<WorldItemObject>())
                    {
                        OnCurrentWorldObjectChanged(null);
                    }

                    break;
            }

            _hudService.SetInfoTitle(string.Empty);
        }

        private void HandleCamera()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            var lookInput = _inputService.PlayerActions.Look.ReadValue<Vector2>() * _settingsService.CameraSensitivity.Value / 100;

            NetworkObject.transform.Rotate(Vector3.up * lookInput.x);
            _cameraPitch -= lookInput.y;
            _cameraPitch = Mathf.Clamp(_cameraPitch, _cameraConfig.PitchLimits.x, _cameraConfig.PitchLimits.y);
            _camera.transform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);

            _camera.Lens.FieldOfView = Mathf.Lerp(_settingsService.Fov.Value, _settingsService.Fov.Value * _cameraConfig.FovSprintModifier, _sprintLerp);
        }
    }
}