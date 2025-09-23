using Cysharp.Threading.Tasks;
using Island.Common.Services;
using Island.Gameplay.Configs.Stats;
using Island.Gameplay.Panels.HUD;
using Island.Gameplay.Panels.Inventory;
using Island.Gameplay.Profiles.Stats;
using JetBrains.Annotations;
using TendedTarsier.Core.Panels;
using TendedTarsier.Core.Services;
using TendedTarsier.Core.Services.Input;
using TendedTarsier.Core.Services.Modules;
using TendedTarsier.Core.Services.Profile;
using UniRx;
using UnityEngine.EventSystems;
using Zenject;

namespace Island.Gameplay.Services.HUD
{
    [UsedImplicitly]
    public class HUDService : ServiceBase, IInitializable
    {
        private EventSystem _eventSystem;
        private ProfileService _profileService;
        private InputService _inputService;
        private ModuleService _moduleService;
        private NetworkService _networkService;
        private PanelLoader<InventoryPanel> _inventoryPanel;
        private PanelLoader<HUDPanel> _hudPanel;

        [Inject]
        private void Construct(
            EventSystem eventSystem,
            ProfileService profileService,
            InputService inputService,
            ModuleService moduleService,
            NetworkService networkService,
            PanelLoader<InventoryPanel> inventoryPanel,
            PanelLoader<HUDPanel> hudPanel)
        {
            _hudPanel = hudPanel;
            _inventoryPanel = inventoryPanel;
            _networkService = networkService;
            _moduleService = moduleService;
            _inputService = inputService;
            _profileService = profileService;
            _eventSystem = eventSystem;
        }

        public void Initialize()
        {
            SubscribeOnInput();
        }

        private void SubscribeOnInput()
        {
            _inputService.OnOptionsButtonPerformed
                .Subscribe(_ => SwitchInventory().Forget())
                .AddTo(CompositeDisposable);

            _inputService.OnMenuButtonPerformed
                .Subscribe(_ => OnMenuButtonClick())
                .AddTo(CompositeDisposable);

            _eventSystem.SetSelectedGameObject(_hudPanel.Instance.SelectedItem.gameObject);
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

        private void OnMenuButtonClick()
        {
            _profileService.SaveAll();
            _networkService.Shutdown();
            _moduleService.LoadModule(_hudPanel.Instance.MenuScene).Forget();
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