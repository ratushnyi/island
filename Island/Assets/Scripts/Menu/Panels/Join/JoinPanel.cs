using Cysharp.Threading.Tasks;
using DG.Tweening;
using TendedTarsier.Core.Panels;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Island.Menu.Panels.Join
{
    public class JoinPanel : ResultPanelBase<string>
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _buttonAccept;
        [SerializeField] private TMP_InputField _textField;
        Tween _tween;
        
        public override async UniTask ShowAnimation()
        {
            await base.ShowAnimation();
            
            _closeButton.OnClickAsObservable().Subscribe(t => Decline()).AddTo(this);
            _buttonAccept.OnClickAsObservable().Subscribe(t => Accept()).AddTo(this);
            _textField.Select();
        }
        
        private void Accept()
        {
            if (_textField.text.Length != 6)
            {
                _tween?.Kill(true);
                _tween = _textField.targetGraphic.DOColor(Color.red, 0.1f)
                    .SetEase(Ease.InOutSine)
                    .SetUpdate(true)
                    .SetLoops(10, LoopType.Yoyo)
                    .OnComplete(() => {
                        _textField.targetGraphic.color = Color.white;
                    });
                return;
            }
            
            HideWithResult(_textField.text.ToUpper());
        }
        
        private void Decline()
        {
            HideWithResult(null);
        }

        private void OnDestroy()
        {
            _tween?.Kill(true);
        }
    }
}