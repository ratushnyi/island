using JetBrains.Annotations;
using TendedTarsier.Core.Services;
using TendedTarsier.Core.Services.Input;
using TendedTarsier.Core.Utilities.Extensions;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.CameraInput
{
    [UsedImplicitly]
    public class CameraInputService : ServiceBase
    {
        [Inject] private InputService _inputService;

        private int _fingerId = -1;
        
        public Vector2 GetCameraInput()
        {
            if (!Application.isMobilePlatform)
            {
                return _inputService.PlayerActions.Look.ReadValue<Vector2>();
            }
            
            if (_fingerId == -1)
            {
                foreach (var touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Began && !InputExtensions.IsOverUI(touch.fingerId) && touch.position.x > Screen.width / 2f)
                    {
                        _fingerId = touch.fingerId;
                    }
                }
            }

            if (_fingerId != -1)
            {
                Touch touch = default;
                foreach (var t in Input.touches)
                {
                    if (t.fingerId == _fingerId)
                    {
                        touch = t; 
                        break;
                    }
                }

                if (touch.phase is TouchPhase.Moved or TouchPhase.Stationary)
                {
                    return touch.deltaPosition;
                }

                if (touch.phase is TouchPhase.Ended or TouchPhase.Canceled)
                {
                    _fingerId = -1;
                }
            }

            return default;
        }
    }
}