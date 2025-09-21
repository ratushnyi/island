using Cysharp.Threading.Tasks;
using TendedTarsier.Core.Panels;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Island.Menu.Panels.Join
{
    public class JoinPanel : PanelBase
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _buttonAccept;
        [SerializeField] private TMP_InputField _textField;
        public readonly UniTaskCompletionSource<string> Result = new();
        
        public override async UniTask ShowAnimation()
        {
            await base.ShowAnimation();
            
            _closeButton.OnClickAsObservable().Subscribe(t => Decline()).AddTo(CompositeDisposable);
            _buttonAccept.OnClickAsObservable().Subscribe(t => Accept()).AddTo(CompositeDisposable);
        }
        
        private void Accept()
        {
            Result.TrySetResult(_textField.text);
            PerformHide();
        }
        
        private void Decline()
        {
            Result.TrySetResult(null);
            PerformHide();
        }
    }
}