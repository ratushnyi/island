using System;
using JetBrains.Annotations;
using TendedTarsier.Core.Services.Input;
using UniRx;
using Zenject;

namespace Island.Gameplay.Player
{
    public interface IPlayerPlatformInput
    {
        public IReadOnlyReactiveProperty<bool> IsSprintInput { get; }

        public void OnSprintClick()
        {
        }

        public void OnSprintPerformed(bool value)
        {
        }
    }

    [UsedImplicitly]
    public class PlayerMobileInput : IPlayerPlatformInput
    {
        public IReadOnlyReactiveProperty<bool> IsSprintInput => _isSprintInput;
        private readonly IReactiveProperty<bool> _isSprintInput = new ReactiveProperty<bool>();

        public void OnSprintClick()
        {
            _isSprintInput.Value = !_isSprintInput.Value;
        }

        public void OnSprintPerformed(bool value)
        {
            _isSprintInput.Value = value;
        }
    }

    [UsedImplicitly]
    public class PlayerConsoleInput : IPlayerPlatformInput
    {
        public IReadOnlyReactiveProperty<bool> IsSprintInput => _isSprintInput;
        private readonly IReactiveProperty<bool> _isSprintInput = new ReactiveProperty<bool>();

        public void OnSprintClick()
        {
            _isSprintInput.Value = !_isSprintInput.Value;
        }

        public void OnSprintPerformed(bool value)
        {
            _isSprintInput.Value = value;
        }
    }

    [UsedImplicitly]
    public class PlayerDesktopInput : IPlayerPlatformInput, IDisposable
    {
        public IReadOnlyReactiveProperty<bool> IsSprintInput => _isSprintInput;
        private readonly IReactiveProperty<bool> _isSprintInput = new ReactiveProperty<bool>();
        private readonly CompositeDisposable _compositeDisposable = new();

        [Inject]
        private void Construct(InputService inputService)
        {
            inputService.OnSprintButtonStarted.Subscribe(_ => _isSprintInput.Value = true).AddTo(_compositeDisposable);
            inputService.OnSprintButtonCanceled.Subscribe(_ => _isSprintInput.Value = false).AddTo(_compositeDisposable);
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}