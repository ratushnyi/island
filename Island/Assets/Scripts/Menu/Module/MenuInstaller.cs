using Island.Common;
using Island.Gameplay.Settings;
using TendedTarsier.Core.Utilities.Extensions;
using UnityEngine;
using Zenject;

namespace Island.Menu.Module
{
    public class MenuInstaller : MonoInstaller
    {
        [SerializeField] private CameraConfig _cameraConfig;

        public override void InstallBindings()
        {
            BindServices();
            BindConfigs();
        }

        private void BindServices()
        {
            Container.BindService<SettingsService>();
        }

        private void BindConfigs()
        {
            Container.Bind<CameraConfig>().FromInstance(_cameraConfig).AsSingle().NonLazy();
        }
    }
}