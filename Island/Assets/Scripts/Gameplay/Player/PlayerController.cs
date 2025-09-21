using Island.Common;
using Island.Common.Services;
using Island.Gameplay.Services;
using Island.Gameplay.Services.HUD;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Settings;
using TendedTarsier.Core.Services.Input;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Player
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private CharacterController _characterController;

        private InputService _inputService;
        private EnergyService _energyService;
        private SettingsService _settingsService;
        private InventoryService _inventoryService;
        private HUDService _hudService;
        private PlayerConfig _playerConfig;
        private CameraConfig _cameraConfig;

        private float _cameraPitch;
        private float _verticalVelocity;
        private float _sprintLerp;

        private bool IsSprintButtonToggle => Application.isMobilePlatform;
        private bool SprintButtonToggleState
        {
            get => IsSprintButtonToggle && _sprintButtonToggleState;
            set => _sprintButtonToggleState = value;
        }

        private bool _sprintButtonToggleState;

        [Inject]
        private void Construct(InputService inputService, EnergyService energyService, SettingsService settingsService, InventoryService inventoryService, HUDService hudService, PlayerConfig playerConfig, CameraConfig cameraConfig)
        {
            _inputService = inputService;
            _energyService = energyService;
            _settingsService = settingsService;
            _inventoryService = inventoryService;
            _hudService = hudService;
            _playerConfig = playerConfig;
            _cameraConfig = cameraConfig;
        }

        private void Start()
        {
            if (!IsOwner)
            {
                return;
            }

            _camera.fieldOfView = _settingsService.Fov;

            Observable.EveryUpdate().Subscribe(OnTick).AddTo(this);
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
            if (_inputService.PlayerActions.Jump.WasPressedThisFrame())
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
            var moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y + Vector3.up * _verticalVelocity;
            moveDirection *= deltaTime;

            _characterController.Move(moveDirection);
        }

        private void HandleSprint(float deltaTime)
        {
            if(_inputService.PlayerActions.Sprint.WasPressedThisFrame())
            {
                if (SprintButtonToggleState)
                {
                    SprintButtonToggleState = false;
                    return;
                }

                SprintButtonToggleState = true;
            }
            
            if ((_inputService.PlayerActions.Sprint.IsPressed() || SprintButtonToggleState) && _energyService.TrackSprint(deltaTime))
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
            switch (other.tag)
            {
                case CommonConstants.ItemTag:
                    var mapItem = other.GetComponent<WorldItemObject>();
                    _hudService.SetInfoTitle(mapItem.ItemEntity.Id);
                    break;
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            _hudService.SetInfoTitle(string.Empty);
        }

        private void HandleCamera()
        {
            var lookInput = _inputService.PlayerActions.Look.ReadValue<Vector2>() * _settingsService.CameraSensitivity / 100;

            transform.Rotate(Vector3.up * lookInput.x);
            _cameraPitch -= lookInput.y;
            _cameraPitch = Mathf.Clamp(_cameraPitch, _cameraConfig.PitchLimits.x, _cameraConfig.PitchLimits.y);
            _camera.transform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);

            _camera.fieldOfView = Mathf.Lerp(_settingsService.Fov, _settingsService.Fov * _cameraConfig.FovSprintModifier, _sprintLerp);
        }
    }
}