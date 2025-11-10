using Cysharp.Threading.Tasks;
using DG.Tweening;
using TendedTarsier.Core.Panels;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Island.Menu.Panels.Join
{
    public class JoinPopup : ResultPopupBase<string>
    {
        [SerializeField] private Button _buttonAccept;
        [SerializeField] private TMP_InputField _textField;
        Tween _tween;

        public override async UniTask ShowAnimation()
        {
            await base.ShowAnimation();

            _buttonAccept.OnClickAsObservable().Subscribe(_ => Accept()).AddTo(this);
            _buttonAccept.interactable = false;
            _textField.onValueChanged.AsObservable().Subscribe(OnText).AddTo(this);
            _textField.Select();
        }

        private void OnText(string text)
        {
            _buttonAccept.interactable = text.Length == 6;
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
                    .OnComplete(() => { _textField.targetGraphic.color = Color.white; });
                return;
            }

            HideWithResult(_textField.text.ToUpper());
        }

        private void OnDestroy()
        {
            _tween?.Kill(true);
        }
    }
}