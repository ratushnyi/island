using Cysharp.Threading.Tasks;
using TendedTarsier.Core.Modules.Menu;
using TendedTarsier.Core.Panels;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Island.Menu.Panels
{
    public class IslandMenuPanel : MenuPanel
    {
        [SerializeField] private Button _settingsButton;
        private PanelLoader<SettingsPanel.SettingsPanel> _settingsPanel;

        [Inject]
        private void Construct(PanelLoader<SettingsPanel.SettingsPanel> settingsPanel)
        {
            _settingsPanel = settingsPanel;
        }

        protected override void InitButtons()
        {
            base.InitButtons();
            RegisterButton(_settingsButton);
        }

        protected override void SubscribeButtons()
        {
            base.SubscribeButtons();

            _settingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsButtonClick()).AddTo(this);
        }

        private void OnSettingsButtonClick()
        {
            _settingsPanel.Show().Forget();
        }
    }
}