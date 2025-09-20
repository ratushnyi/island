using Island.Gameplay.Settings;
using JetBrains.Annotations;
using TendedTarsier.Core.Services;
using UnityEngine;
using Zenject;

namespace Island.Common
{
    [UsedImplicitly]
    public class SettingsService : ServiceBase
    {
        private CameraConfig _cameraConfig;

        [Inject]
        private void Construct(CameraConfig cameraConfig)
        {
            _cameraConfig = cameraConfig;
        }

        public int Fov
        {
            get => PlayerPrefs.GetInt("Fov", _cameraConfig.DefaultFov);
            set => PlayerPrefs.SetInt("Fov", value);
        }

        public int CameraSensitivity
        {
            get => PlayerPrefs.GetInt("CameraSensitivity", _cameraConfig.DefaultCameraSensitivity);
            set => PlayerPrefs.SetInt("CameraSensitivity", value);
        }
    }
}