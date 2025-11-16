using Island.Common.Services;
using Island.Gameplay.Settings;
using TendedTarsier.Core.Utilities.Extensions;
using UnityEngine;
using Zenject;

namespace Island.Common
{
    public class CommonInstaller : MonoInstaller
    {
        [SerializeField] private CameraConfig _cameraConfig;

        public override void InstallBindings()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = 60;

            BindConfigs();
            BindServices();
        }

        private void BindConfigs()
        {
            Container.Bind<CameraConfig>().FromInstance(_cameraConfig).AsSingle().NonLazy();
        }

        private void BindServices()
        {
            Container.BindService<SettingsService>();
            Container.BindService<MatchmakingService>();
        }
    }
}