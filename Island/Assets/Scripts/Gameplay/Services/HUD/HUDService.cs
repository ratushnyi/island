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
using TendedTarsier.Core.Utilities.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Island.Gameplay.Services.HUD
{
    [UsedImplicitly]
    public class HUDService : ServiceBase, INetworkInitialize
    {
        private EventSystem _eventSystem;
        private InputService _inputService;
        private BackButtonService _backButtonService;
        private NetworkService _networkService;
        private AimService _aimService;
        private PanelLoader<HUDPanel> _hudPanel;
        private PanelLoader<PausePopup> _pausePanel;
        private PanelLoader<InventoryPopup> _inventoryPanel;
        private IslandGameplayModuleController _islandGameplayModuleController;

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
            _pausePanel = pausePanel;
            _hudPanel = hudPanel;
            _aimService = aimService;
            _networkService = networkService;
            _inputService = inputService;
            _backButtonService = backButtonService;
            _eventSystem = eventSystem;
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

            InputExtensions.EnableCursor(false);
            
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
                var panel = await _pausePanel.Show();
                InputExtensions.EnableCursor(true);
                var isExit = await panel.WaitForResult();
                if (isExit)
                {
                    _islandGameplayModuleController.LoadMenu().Forget();
                    return;
                }
            }
            else if (_pausePanel.Instance != null)
            {
                _pausePanel.Hide().Forget();
            }
            
            InputExtensions.EnableCursor(false);
        }

        private async UniTaskVoid HandlePause()
        {
            _networkService.SetPaused(true);
            var panel = await _pausePanel.Show();
            InputExtensions.EnableCursor(true);
            var isExit = await panel.WaitForResult();
            _networkService.SetPaused(false);
            if (isExit)
            {
                _islandGameplayModuleController.LoadMenu().Forget();
                return;
            }

            InputExtensions.EnableCursor(false);
        }

        private void SubscribeOnPause()
        {
            _backButtonService.AddAction(() => HandlePause().Forget()).AddTo(CompositeDisposable);
        }

        private async UniTask SwitchInventory()
        {
            if (_inventoryPanel.Instance != null)
            {
                InputExtensions.EnableCursor(false);
                await _inventoryPanel.Hide();
            }
            else
            {
                await _inventoryPanel.Show();
                InputExtensions.EnableCursor(true);
                _eventSystem.SetSelectedGameObject(_inventoryPanel.Instance.FirstCellView.gameObject);
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