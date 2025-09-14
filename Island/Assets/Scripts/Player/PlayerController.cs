using Module;
using TendedTarsier.Core.Services.Input;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Player
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private float _jumpForce = 5;
        [SerializeField] private float _gravity = 0.00001f;
        [SerializeField] private float _walkSpeed = 0.0001f;
        [SerializeField] private float _sprintSpeed = 0.0002f;
        [SerializeField] private float _sprintLerpSpeed = 1f;
        
        [Header("Camera")]
        [SerializeField] private Camera _camera;
        [SerializeField] private float _fovSprintModifier = 1.1f;
        [SerializeField] private  Vector2 _pitchLimits = new(-85f, 85f);

        [Header("Settings")] 
        [SerializeField] private float _fov = 75;
        [SerializeField] private float _cameraSensitivity = 0.1f;
        
        private InputService _inputService;
        
        private float _cameraPitch;
        private float _verticalVelocity;
        private float _sprintLerp;
        private EnergyService _energyService;

        [Inject]
        private void Construct(InputService inputService, EnergyService energyService)
        {
            _inputService = inputService;
            _energyService = energyService;
        }

        private void Start()
        {
            if (!IsOwner)
            {
                return;
            }
            
            _camera.fieldOfView = _fov;
            
            Observable.EveryUpdate().Subscribe(OnTick).AddTo(this);
        }

        private void OnTick(long deltaTime)
        {
            HandleSprintLerp(deltaTime);
            HandleVerticalVelocity(deltaTime);
            HandleMove(deltaTime);
            HandleCamera();
        }

        private void HandleVerticalVelocity(long deltaTime)
        {
            if (_inputService.PlayerActions.Jump.WasPressedThisFrame())
            {
                _verticalVelocity = _jumpForce;
            }
            else if (!_characterController.isGrounded)
            {
                _verticalVelocity -= _gravity * deltaTime;
            }
            else
            {
                _verticalVelocity = -_gravity;
            }
        }
        
        private void HandleMove(float deltaTime)
        {
            var movementSpeed = Mathf.Lerp(_walkSpeed, _sprintSpeed, _sprintLerp);
            var moveInput = _inputService.PlayerActions.Move.ReadValue<Vector2>() * deltaTime * movementSpeed;
            var moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y + Vector3.up * _verticalVelocity;
            
            _characterController.Move(moveDirection);
        }

        private void HandleSprintLerp(float deltaTime)
        {
            if (_inputService.PlayerActions.Sprint.IsPressed())
            {
                _sprintLerp = Mathf.Lerp(_sprintLerp, 1, deltaTime * _sprintLerpSpeed);
                _energyService.TrackSprint(deltaTime);
            }
            else
            {
                _sprintLerp = 0;
            }
        }
        
        private void HandleCamera()
        {
            var lookInput = _inputService.PlayerActions.Look.ReadValue<Vector2>() * _cameraSensitivity;
            
            transform.Rotate(Vector3.up * lookInput.x);
            _cameraPitch -= lookInput.y;
            _cameraPitch = Mathf.Clamp(_cameraPitch, _pitchLimits.x, _pitchLimits.y);
            _camera.transform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
            
            _camera.fieldOfView = Mathf.Lerp(_fov, _fov * _fovSprintModifier, _sprintLerp);
        }
    }
}
