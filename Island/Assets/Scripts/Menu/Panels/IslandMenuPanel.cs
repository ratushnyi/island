using Cysharp.Threading.Tasks;
using Island.Common.Services;
using Island.Menu.Panels.Join;
using Island.Menu.Panels.Settings;
using TendedTarsier.Core.Modules.Menu;
using TendedTarsier.Core.Panels;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Island.Menu.Panels
{
    public class IslandMenuPanel : MenuPanel
    {
        [SerializeField] private Button _joinButton;
        [SerializeField] private Button _settingsButton;
        [Inject] private PanelLoader<SettingsPopup> _settingsPanel;
        [Inject] private PanelLoader<JoinPopup> _joinPanel;
        [Inject] private MatchmakingService _matchmakingService;

        protected override void InitButtons()
        {
            _matchmakingService.EnsureServices().Forget();
            base.InitButtons();
            RegisterButton(_settingsButton);
            InitContinueButton(!string.IsNullOrEmpty(ProjectProfile.ServerId));
        }

        protected override void SubscribeButtons()
        {
            base.SubscribeButtons();
            _joinButton.OnClickAsObservable().Subscribe(_ => OnJoinButtonClick().Forget()).AddTo(this);
            _settingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsButtonClick()).AddTo(this);
        }
        
        protected override async UniTask OnContinueButtonClick()
        {
            base.OnContinueButtonClick().Forget();
            await _matchmakingService.StartHost(false);
        }

        protected override async UniTask OnNewGameButtonClick()
        {
            ProfileService.ClearServerSave(ProjectProfile.ServerId);
            ModuleService.LoadModule(ProjectConfig.GameplayScene).Forget();
            await _matchmakingService.StartHost(true);
        }

        private async UniTask OnJoinButtonClick()
        {
            var panel = await _joinPanel.Show();
            var joinCode = await panel.WaitForResult();
            if (string.IsNullOrEmpty(joinCode))
            {
                return;
            }

            await _matchmakingService.TryStartClient(joinCode, beforeClientStarted);

            async UniTask beforeClientStarted()
            {
                base.OnContinueButtonClick().Forget();
                
                await UniTask.WaitUntil(() => SceneManager.GetActiveScene().name == ProjectConfig.GameplayScene);
            }
        }

        private void OnSettingsButtonClick()
        {
            _settingsPanel.Show().Forget();
        }
    }
}