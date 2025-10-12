using Island.Menu.Panels.Join;
using Island.Menu.Panels.Settings;
using TendedTarsier.Core.Utilities.Extensions;
using UnityEngine;
using Zenject;

namespace Island.Menu.Module
{
    public class IslandMenuInstaller : MonoInstaller
    {
        [SerializeField] private JoinPopup _joinPopup;
        [SerializeField] private SettingsPopup _settingsPopup;
        [SerializeField] private Canvas _canvas;

        public override void InstallBindings()
        {
            BindPanels();
        }

        private void BindPanels()
        {
            Container.BindPanel(_joinPopup, _canvas);
            Container.BindPanel(_settingsPopup, _canvas);
        }
    }
}