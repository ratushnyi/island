using Island.Menu.Panels.Join;
using Island.Menu.Panels.Settings;
using TendedTarsier.Core.Utilities.Extensions;
using UnityEngine;
using Zenject;

namespace Island.Menu.Module
{
    public class IslandMenuInstaller : MonoInstaller
    {
        [SerializeField] private JoinPanel _joinPanel;
        [SerializeField] private SettingsPanel _settingsPanel;
        [SerializeField] private Canvas _canvas;

        public override void InstallBindings()
        {
            BindPanels();
        }

        private void BindPanels()
        {
            Container.BindPanel(_joinPanel, _canvas);
            Container.BindPanel(_settingsPanel, _canvas);
        }
    }
}