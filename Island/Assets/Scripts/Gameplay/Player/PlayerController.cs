using System;
using Island.Common.Services;
using Island.Gameplay.Services.CameraInput;
using Island.Gameplay.Services.Stats;
using Island.Gameplay.Settings;
using TendedTarsier.Core.Panels;
using TendedTarsier.Core.Services.Input;
using TendedTarsier.Core.Utilities.Extensions;
using UniRx;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Player
{
    public class PlayerController : NetworkBehaviour
    {
        private readonly int _jumpingAnimatorKey = Animator.StringToHash("Jumping");
        private readonly int _speedAnimatorKey = Animator.StringToHash("Speed");
        private readonly int _sideSpeedAnimatorKey = Animator.StringToHash("SideSpeed");

        [SerializeField] private Transform _head;
        [SerializeField] private Animator _animator;
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private CharacterController _characterController;

        [Inject] private CameraInputService _cameraInputService;
        [Inject] private InputService _inputService;
        [Inject] private StatsService _statService;
        [Inject] private SettingsService _settingsService;
        [Inject] private PanelService _panelService;
        [Inject] private PlayerService _playerService;
        [Inject] private PlayerConfig _playerConfig;
        [Inject] private CameraConfig _cameraConfig;

        private Ray _aimRay;
        private float _cameraPitch;
        private float _verticalVelocity;
        private float _sprintLerp;
        private bool _isRunning;
        private float? _jumpingDelay;
        private bool IsMoving => _characterController.velocity.magnitude > 0;
        private bool IsRunningInput => IsMoving && (_inputService.PlayerActions.Sprint.IsPressed() || SprintButtonToggleState);

        public IObservable<Vector3> OnPositionChanged => _onPositionChanged;
        private readonly ISubject<Vector3> _onPositionChanged = new Subject<Vector3>();
        public IObservable<Quaternion> OnRotationChanged => _onRotationChanged;
        private readonly ISubject<Quaternion> _onRotationChanged = new Subject<Quaternion>();

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
            Observable.EveryLateUpdate().Subscribe(HandleCamera).AddTo(this);
            _settingsService.Fov.Subscribe(OnFovChanged).AddTo(this);
        }

        private void OnFovChanged(int value)
        {
            _cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(value, value * _cameraConfig.FovSprintModifier, _sprintLerp);
        }

        private void OnTick(long frame)
        {
            HandleSprint(Time.deltaTime);
            HandleVerticalVelocity(Time.deltaTime);
            HandleMove(Time.deltaTime);
        }

        private void HandleVerticalVelocity(float deltaTime)
        {
            if (_inputService.PlayerActions.Jump.WasPressedThisFrame() && _characterController.isGrounded && !_jumpingDelay.HasValue)
            {
                _jumpingDelay = _playerConfig.JumpDelay;
                _animator.SetBool(_jumpingAnimatorKey, true);
            }
            else if (_jumpingDelay is > 0)
            {
                _jumpingDelay -= deltaTime;
            }
            else if (_jumpingDelay is < 0)
            {
                _jumpingDelay = null;
                _verticalVelocity = _playerConfig.JumpForce;
            }
            else if (!_characterController.isGrounded)
            {
                _verticalVelocity -= _playerConfig.Gravity * deltaTime;
            }
            else
            {
                _animator.SetBool(_jumpingAnimatorKey, false);
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
            var moveInput = _inputService.PlayerActions.Move.ReadValue<Vector2>();
            if (_isRunning)
            {
                moveInput.x = 0;
            }
            var moveSpeed = moveInput * movementSpeed;
            var moveDirection = NetworkObject.transform.right * moveSpeed.x + NetworkObject.transform.forward * moveSpeed.y + Vector3.up * _verticalVelocity;
            moveDirection *= deltaTime;
            _characterController.Move(moveDirection);

            if (moveInput.magnitude > 0 || !_characterController.isGrounded)
            {
                _onPositionChanged.OnNext(NetworkObject.transform.position);
            }

            if (moveInput.magnitude > 0 && _characterController.isGrounded)
            {
                _animator.SetFloat(_speedAnimatorKey, moveInput.y + _sprintLerp);
                _animator.SetFloat(_sideSpeedAnimatorKey, moveInput.x);
            }
            else
            {
                _animator.SetFloat(_sideSpeedAnimatorKey, 0);
                _animator.SetFloat(_speedAnimatorKey, 0);
            }
        }

        private void HandleSprint(float deltaTime)
        {
            if (_panelService.IsAnyPopupOpen)
            {
                return;
            }

            var wrongDirection = _inputService.PlayerActions.Move.ReadValue<Vector2>().y <= 0;

            if (_inputService.PlayerActions.Sprint.WasPressedThisFrame())
            {
                if (SprintButtonToggleState || !IsMoving || wrongDirection)
                {
                    SprintButtonToggleState = false;
                    _isRunning = false;
                    return;
                }

                _playerConfig.SprintFee.Deposit = 0;
            }

            if (IsRunningInput && _statService.TrackFee(_playerConfig.SprintFee, deltaTime) && !wrongDirection)
            {
                SprintButtonToggleState = true;
                _isRunning = true;
                _sprintLerp = Mathf.Lerp(_sprintLerp, 1, deltaTime * _playerConfig.SprintGetSpeed);
            }
            else
            {
                SprintButtonToggleState = false;
                _isRunning = false;
                _sprintLerp = Mathf.Lerp(_sprintLerp, 0, deltaTime * _playerConfig.SprintFallSpeed);
            }
        }

        private void HandleCamera(long frame)
        {
            if (_panelService.IsAnyPopupOpen)
            {
                return;
            }

            var cameraFovSprintModifier = Mathf.Lerp(1, _cameraConfig.FovSprintModifier, _sprintLerp);
            var lookInput = _cameraInputService.GetCameraInput() * _settingsService.CameraSensitivity.Value / 100;

            NetworkObject.transform.Rotate(Vector3.up * lookInput.x);
            _cameraPitch -= lookInput.y;
            _cameraPitch = Mathf.Clamp(_cameraPitch, _cameraConfig.PitchLimits.x / cameraFovSprintModifier, _cameraConfig.PitchLimits.y / cameraFovSprintModifier);
            _head.transform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
            _head.transform.rotation = Quaternion.Euler(_head.transform.rotation.eulerAngles.x, _head.transform.rotation.eulerAngles.y, 0f);
            _cinemachineCamera.Lens.FieldOfView = _settingsService.Fov.Value * cameraFovSprintModifier;

            if (lookInput.magnitude > 0)
            {
                _onRotationChanged.OnNext(NetworkObject.transform.rotation);
            }
        }
    }
}