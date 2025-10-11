using Cysharp.Threading.Tasks;
using Island.Common.Services;
using Island.Menu.Panels.Join;
using Island.Menu.Panels.Settings;
using TendedTarsier.Core.Modules.Menu;
using TendedTarsier.Core.Modules.Project;
using TendedTarsier.Core.Panels;
using TendedTarsier.Core.Services.Modules;
using TendedTarsier.Core.Services.Profile;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Island.Menu.Panels
{
    public class IslandMenuPanel : MenuPanel
    {
        [SerializeField] private Button _joinButton;
        [SerializeField] private Button _settingsButton;
        private PanelLoader<SettingsPanel> _settingsPanel;
        private PanelLoader<JoinPanel> _joinPanel;
        private ModuleService _moduleService;
        private NetworkService _networkService;
        private ProfileService _profileService;
        private ProjectProfile _projectProfile;
        private ProjectConfig _projectConfig;

        [Inject]
        private void Construct(PanelLoader<SettingsPanel> settingsPanel, PanelLoader<JoinPanel> joinPanel, NetworkService networkService, ModuleService moduleService, ProfileService profileService, ProjectProfile projectProfile, ProjectConfig projectConfig)
        {
            _settingsPanel = settingsPanel;
            _joinPanel = joinPanel;
            _networkService = networkService;
            _moduleService = moduleService;
            _profileService = profileService;
            _projectProfile = projectProfile;
            _projectConfig = projectConfig;
        }

        protected override void InitButtons()
        {
            _networkService.EnsureServices().Forget();
            base.InitButtons();
            RegisterButton(_settingsButton);
            InitContinueButton(!string.IsNullOrEmpty(_projectProfile.ServerId));
        }

        protected override void SubscribeButtons()
        {
            base.SubscribeButtons();
            _joinButton.OnClickAsObservable().Subscribe(_ => OnJoinButtonClick().Forget()).AddTo(this);
            _settingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsButtonClick()).AddTo(this);
        }
        
        protected override async UniTask OnContinueButtonClick()
        {
            await base.OnContinueButtonClick();
            await _networkService.StartHost(false);
        }

        protected override async UniTask OnNewGameButtonClick()
        {
            _profileService.SetNewGame(true);
            await _moduleService.LoadModule(_projectConfig.GameplayScene);
            await _networkService.StartHost(true);
        }

        private async UniTask OnJoinButtonClick()
        {
            var panel = await _joinPanel.Show();
            var joinCode = await panel.WaitForResult();
            if (string.IsNullOrEmpty(joinCode))
            {
                return;
            }

            _profileService.SetNewGame(false);
            await _networkService.TryStartClient(joinCode, base.OnContinueButtonClick);
        }

        private void OnSettingsButtonClick()
        {
            _settingsPanel.Show().Forget();
        }
    }
}