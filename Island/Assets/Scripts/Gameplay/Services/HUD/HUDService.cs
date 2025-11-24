using Cysharp.Threading.Tasks;
using Island.Gameplay.Configs.Stats;
using Island.Gameplay.Module;
using Island.Gameplay.Panels.HUD;
using Island.Gameplay.Panels.Pause;
using Island.Gameplay.Panels.Player;
using Island.Gameplay.Profiles.Stats;
using Island.Gameplay.Services.Server;
using JetBrains.Annotations;
using TendedTarsier.Core.Panels;
using TendedTarsier.Core.Services;
using TendedTarsier.Core.Services.Input;
using UniRx;
using Zenject;

namespace Island.Gameplay.Services.HUD
{
    [UsedImplicitly]
    public class HUDService : ServiceBase, IServerInitialize
    {
        [Inject] private InputService _inputService;
        [Inject] private BackButtonService _backButtonService;
        [Inject] private ServerService _serverService;
        [Inject] private AimService _aimService;
        [Inject] private PanelLoader<HUDPanel> _hudPanel;
        [Inject] private PanelLoader<PausePopup> _pausePopup;
        [Inject] private PanelLoader<PlayerPopup> _playerPanel;
        [Inject] private IslandGameplayModuleController _islandGameplayModuleController;

        public ProgressBar ProgressBar => _hudPanel.Instance.ProgressBar;

        public void OnNetworkInitialize()
        {
            SubscribeOnInput();
            _hudPanel.Instance.OnNetworkInitialize();
            _aimService.TargetObject.Subscribe(t => SetInfoTitle(t?.Name)).AddTo(CompositeDisposable);
        }

        private void SubscribeOnInput()
        {
            _inputService.OnOptionsButtonPerformed.Subscribe(_ => SwitchPlayerPopup().Forget()).AddTo(CompositeDisposable);

            SubscribeOnServerPause();
            SubscribeOnPause();
        }

        private void SubscribeOnServerPause()
        {
            if (!_serverService.IsServer)
            {
                _serverService.IsServerPaused.Subscribe(ProcessServerPause).AddTo(CompositeDisposable);
            }
        }

        private void ProcessServerPause(bool value)
        {
            if (value)
            {
                processServerPauseAsync().Forget();
            }
            else if (_pausePopup.Instance != null)
            {
                _pausePopup.Hide().Forget();
            }

            async UniTaskVoid processServerPauseAsync()
            {
                if (_pausePopup.PanelState.Value is PanelState.Show or PanelState.Showing)
                {
                    _pausePopup.Instance.UpdateActiveCloseButton();
                    return;
                }

                if (_pausePopup.PanelState.Value is PanelState.Hiding)
                {
                    await _pausePopup.PanelState.First();
                }

                HandlePause().Forget();
            }
        }

        private async UniTaskVoid HandlePause()
        {
            _serverService.SetPaused(true);
            var panel = await _pausePopup.Show();
            var isExit = await panel.WaitForResult();
            _serverService.SetPaused(false);
            if (isExit)
            {
                _islandGameplayModuleController.LoadMenu().Forget();
            }
        }

        private void SubscribeOnPause()
        {
            _backButtonService.AddAction(() => HandlePause().Forget()).AddTo(CompositeDisposable);
        }

        private async UniTask SwitchPlayerPopup()
        {
            if (_pausePopup.PanelState.Value != PanelState.Hide)
            {
                return;
            }

            if (_playerPanel.Instance != null)
            {
                await _playerPanel.Hide();
            }
            else
            {
                await _playerPanel.Show();
            }
        }

        private void SetInfoTitle(string title)
        {
            _hudPanel.Instance.GetInfoTitle().UpdateValue(title);
        }

        public void ShowStatBar(StatType statType, StatModel statModel, StatProfileElement statProfile)
        {
            var statBar = _hudPanel.Instance.GetStatBar(statType);

            statBar.Setup(statProfile.Value.Value, statProfile.Range.Value);
            statBar.SetSprite(statModel.Sprite);
            statProfile.Value.SkipLatestValueOnSubscribe().Subscribe(t => statBar.UpdateValue(t)).AddTo(CompositeDisposable);
            statProfile.Range.SkipLatestValueOnSubscribe().Subscribe(t => statBar.UpdateRange(t)).AddTo(CompositeDisposable);
        }
    }
}