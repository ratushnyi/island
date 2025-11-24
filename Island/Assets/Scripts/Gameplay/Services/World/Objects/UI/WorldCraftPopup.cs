using Island.Gameplay.Configs.Craft;
using TendedTarsier.Core.Panels;
using UniRx;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.World.Objects.UI
{
    public class WorldCraftPopup : ResultPopupBase<(CraftReceiptEntity Receipt, int Count)>
    {
        [SerializeField] private CraftView _view;

        [Inject]
        public void Construct(WorldObjectType type)
        {
            _view.Initialize(type);
            _view.OnCraft.Subscribe(HideWithResult).AddTo(this);
        }
    }
}