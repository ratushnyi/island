using Island.Common.Services;
using Island.Menu.Panels.Join;
using Island.Menu.Panels.Settings;
using TendedTarsier.Core.Utilities.Extensions;
using UnityEngine;
using Zenject;

namespace Island.Menu.Module
{
    public class MenuInstaller : MonoInstaller
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
            Container.BindPanel<JoinPanel>(_joinPanel, _canvas);
            Container.BindPanel<SettingsPanel>(_settingsPanel, _canvas);
        }
    }
}