using Island.Gameplay.Settings;
using JetBrains.Annotations;
using TendedTarsier.Core.Services;
using TendedTarsier.Core.Utilities.Extensions;
using Zenject;

namespace Island.Common.Services
{
    [UsedImplicitly]
    public class SettingsService : ServiceBase
    {
        private CameraConfig _cameraConfig;

        [Inject]
        private void Construct(CameraConfig cameraConfig)
        {
            _cameraConfig = cameraConfig;
            
            Fov = new ReactivePrefs<int>("Fov", _cameraConfig.DefaultFov);
            CameraSensitivity = new ReactivePrefs<int>("CameraSensitivity", _cameraConfig.DefaultCameraSensitivity);
        }

        public ReactivePrefs<int> Fov;
        public ReactivePrefs<int> CameraSensitivity;
    }
}