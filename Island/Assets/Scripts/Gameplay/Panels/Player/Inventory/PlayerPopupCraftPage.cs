using System.Threading;
using Cysharp.Threading.Tasks;
using Island.Gameplay.Configs.Craft;
using Island.Gameplay.Services.HUD;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.World.Objects;
using Island.Gameplay.Services.World.Objects.UI;
using TendedTarsier.Core.Panels;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Island.Gameplay.Panels.Player.Inventory
{
    public class PlayerPopupCraftPage : PlayerPopupPage
    {
        public override string Name => "Craft";
        [SerializeField] private CraftView _view;
        [SerializeField] private TMP_Text _progressBarText;
        [SerializeField] private ProgressBar _progressBar;
        [SerializeField] private Button _cancelButton;
        [Inject] private InventoryService _inventory;
        [Inject] private BackButtonService _backButtonService;

        public override void Initialize()
        {
            _view.Initialize(WorldObjectType.Player);
            _view.OnCraft.Subscribe(OnCraft).AddTo(this);
        }

        private void OnCraft((CraftReceiptEntity Receipt, int Count) order)
        {
            if (!_inventory.IsEnoughSpace(order.Receipt.Result))
            {
                return;
            }

            foreach (var ingredientEntity in order.Receipt.Ingredients)
            {
                if (!_inventory.IsEnough(ingredientEntity))
                {
                    return;
                }
            }

            var disposable = new CompositeDisposable();
            var cancellationTokenSource = new CancellationTokenSource();
            _backButtonService.AddAction(() => cancellationTokenSource.Cancel()).AddTo(disposable);
            _cancelButton.OnClickAsObservable().Subscribe(_ => cancellationTokenSource.Cancel()).AddTo(disposable);
            OnCraftAsync(order, cancellationTokenSource, disposable).Forget();
        }

        private async UniTask OnCraftAsync((CraftReceiptEntity Receipt, int Count) order, CancellationTokenSource tokenSource, CompositeDisposable disposable)
        {
            for (int i = 0; i < order.Count; i++)
            {
                _progressBarText.SetText($"{i + 1}/{order.Count}");
                await _progressBar.Show(order.Receipt.Duration, tokenSource.Token);
                if (tokenSource.IsCancellationRequested)
                {
                    break;
                }

                foreach (var ingredientEntity in order.Receipt.Ingredients)
                {
                    _inventory.TryRemove(ingredientEntity);
                }

                _inventory.TryCollect(order.Receipt.Result);
                _view.UpdateReceiptIngredients();
            }

            disposable.Dispose();
        }
    }
}