using Cysharp.Threading.Tasks;
using Island.Common.Services;
using Island.Gameplay.Configs.Stats;
using Island.Gameplay.Module;
using Island.Gameplay.Panels.HUD;
using Island.Gameplay.Panels.Inventory;
using Island.Gameplay.Panels.Pause;
using Island.Gameplay.Profiles.Stats;
using JetBrains.Annotations;
using TendedTarsier.Core.Panels;
using TendedTarsier.Core.Services;
using TendedTarsier.Core.Services.Input;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Island.Gameplay.Services.HUD
{
    [UsedImplicitly]
    public class HUDService : ServiceBase, INetworkInitialize
    {
        private InputService _inputService;
        private BackButtonService _backButtonService;
        private NetworkService _networkService;
        private AimService _aimService;
        private PanelLoader<HUDPanel> _hudPanel;
        private PanelLoader<PausePopup> _pausePopup;
        private PanelLoader<InventoryPopup> _inventoryPanel;
        private IslandGameplayModuleController _islandGameplayModuleController;

        public ProgressBar ProgressBar => _hudPanel.Instance.ProgressBar;

        [Inject]
        private void Construct(
            EventSystem eventSystem,
            InputService inputService,
            BackButtonService backButtonService,
            NetworkService networkService,
            AimService aimService,
            PanelLoader<HUDPanel> hudPanel,
            PanelLoader<PausePopup> pausePanel,
            PanelLoader<InventoryPopup> inventoryPanel,
            IslandGameplayModuleController islandGameplayModuleController)
        {
            _islandGameplayModuleController = islandGameplayModuleController;
            _inventoryPanel = inventoryPanel;
            _pausePopup = pausePanel;
            _hudPanel = hudPanel;
            _aimService = aimService;
            _networkService = networkService;
            _inputService = inputService;
            _backButtonService = backButtonService;
        }

        public void OnNetworkInitialize()
        {
            SubscribeOnInput();
            _hudPanel.Instance.OnNetworkInitialize();
            _aimService.TargetObject.Subscribe(t => SetInfoTitle(t?.Name)).AddTo(CompositeDisposable);
        }

        private void SubscribeOnInput()
        {
            if (Application.isMobilePlatform)
            {
                _hudPanel.Instance.SelectedItem.OnButtonClicked.Subscribe(_ => SwitchInventory().Forget()).AddTo(CompositeDisposable);
            }
            else
            {
                _inputService.OnOptionsButtonPerformed.Subscribe(_ => SwitchInventory().Forget()).AddTo(CompositeDisposable);
            }
            
            SubscribeOnServerPause();
            SubscribeOnPause();
        }

        private void SubscribeOnServerPause()
        {
            if (!_networkService.IsServer)
            {
                _networkService.IsServerPaused.Subscribe(t => HandleServerPause(t).Forget()).AddTo(CompositeDisposable);
            }
        }

        private async UniTaskVoid HandleServerPause(bool value)
        {
            if (value)
            {
                var panel = await _pausePopup.Show();
                var isExit = await panel.WaitForResult();
                if (isExit)
                {
                    _islandGameplayModuleController.LoadMenu().Forget();
                }
            }
            else if (_pausePopup.Instance != null)
            {
                _pausePopup.Hide().Forget();
            }
        }

        private async UniTaskVoid HandlePause()
        {
            _networkService.SetPaused(true);
            var panel = await _pausePopup.Show();
            var isExit = await panel.WaitForResult();
            _networkService.SetPaused(false);
            if (isExit)
            {
                _islandGameplayModuleController.LoadMenu().Forget();
            }
        }

        private void SubscribeOnPause()
        {
            _backButtonService.AddAction(() => HandlePause().Forget()).AddTo(CompositeDisposable);
        }

        private async UniTask SwitchInventory()
        {
            if (_inventoryPanel.Instance != null)
            {
                await _inventoryPanel.Hide();
            }
            else
            {
                await _inventoryPanel.Show();
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