using Island.Common;
using Island.Common.Services;
using Island.Gameplay.Services;
using Island.Gameplay.Services.Stats;
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
        private PlayerConfig _playerConfig;
        private CameraConfig _cameraConfig;
        
        private float _cameraPitch;
        private float _verticalVelocity;
        private float _sprintLerp;

        [Inject]
        private void Construct(InputService inputService, EnergyService energyService, SettingsService settingsService, PlayerConfig playerConfig, CameraConfig cameraConfig)
        {
            _inputService = inputService;
            _energyService = energyService;
            _settingsService = settingsService;
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
            HandleSprintLerp(Time.deltaTime);
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
            var moveInput = _inputService.PlayerActions.Move.ReadValue<Vector2>() * deltaTime * movementSpeed;
            var moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y + Vector3.up * _verticalVelocity;
            
            _characterController.Move(moveDirection);
        }

        private void HandleSprintLerp(float deltaTime)
        {
            if (_inputService.PlayerActions.Sprint.IsPressed() && _energyService.TrackSprint(deltaTime))
            {
                _sprintLerp = Mathf.Lerp(_sprintLerp, 1, deltaTime * _playerConfig.SprintGetSpeed);
            }
            else if (_sprintLerp > 0)
            {
                _sprintLerp = Mathf.Lerp(_sprintLerp, 0, deltaTime * _playerConfig.SprintFallSpeed);
            }
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
