using System;
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
using UnityEngine.EventSystems;
using Zenject;

namespace Island.Gameplay.Services.HUD
{
    [UsedImplicitly]
    public class HUDService : ServiceBase, INetworkInitialize
    {
        private EventSystem _eventSystem;
        private InputService _inputService;
        private NetworkService _networkService;
        private PanelLoader<PausePanel> _pausePanel;
        private PanelLoader<InventoryPanel> _inventoryPanel;
        private PanelLoader<HUDPanel> _hudPanel;
        private IslandGameplayModuleController _islandGameplayModuleController;

        private IDisposable _pauseDisposable;

        [Inject]
        private void Construct(
            EventSystem eventSystem,
            InputService inputService,
            NetworkService networkService,
            PanelLoader<PausePanel> pausePanel,
            PanelLoader<InventoryPanel> inventoryPanel,
            PanelLoader<HUDPanel> hudPanel,
            IslandGameplayModuleController islandGameplayModuleController)
        {
            _islandGameplayModuleController = islandGameplayModuleController;
            _hudPanel = hudPanel;
            _inventoryPanel = inventoryPanel;
            _pausePanel = pausePanel;
            _networkService = networkService;
            _inputService = inputService;
            _eventSystem = eventSystem;
        }

        public void OnNetworkInitialize()
        {
            SubscribeOnInput();
            _hudPanel.Instance.OnNetworkInitialize();
        }

        private void SubscribeOnInput()
        {
            _inputService.OnOptionsButtonPerformed
                .Subscribe(_ => SwitchInventory().Forget())
                .AddTo(CompositeDisposable);

            _eventSystem.SetSelectedGameObject(_hudPanel.Instance.SelectedItem.gameObject);

            SubscribeOnServerPause();
            SubscribeOnPause();
        }

        private void SubscribeOnServerPause()
        {
            if (!_networkService.IsServer)
            {
                _networkService.IsServerPaused.Subscribe(HandleServerPause).AddTo(CompositeDisposable);
            }
        }

        private void HandleServerPause(bool value)
        {
            if (value)
            {
                ShowPause().Forget();
            }
            else if (_pausePanel.Instance != null)
            {
                _pausePanel.Hide().Forget();
            }
        }

        private async UniTask<bool> ShowPause()
        {
            var panel = await _pausePanel.Show();
            var isExit = await panel.WaitForResult();
            if (isExit)
            {
                _islandGameplayModuleController.LoadMenu();
            }

            return isExit;
        }

        private async UniTaskVoid HandlePause()
        {
            if (_pauseDisposable != null)
            {
                CompositeDisposable.Remove(_pauseDisposable);
                _pauseDisposable.Dispose();
                _networkService.SetPaused(true);
                var isExit = await ShowPause();
                _networkService.SetPaused(false);
                if(!isExit)
                {
                    SubscribeOnPause();
                }
            }
        }

        private void SubscribeOnPause()
        {
            _pauseDisposable = _inputService.OnMenuButtonPerformed.Subscribe(_ => HandlePause().Forget())
                .AddTo(CompositeDisposable);
        }

        private async UniTask SwitchInventory()
        {
            if (_inventoryPanel.Instance != null)
            {
                await _inventoryPanel.Hide();
                _eventSystem.SetSelectedGameObject(_hudPanel.Instance.SelectedItem.gameObject);
            }
            else
            {
                await _inventoryPanel.Show();
                _eventSystem.SetSelectedGameObject(_inventoryPanel.Instance.FirstCellView.gameObject);
            }
        }

        public void SetInfoTitle(string title)
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