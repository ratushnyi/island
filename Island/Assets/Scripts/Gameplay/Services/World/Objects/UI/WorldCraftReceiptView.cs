using System;
using Island.Gameplay.Configs.Craft;
using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Panels.Player.Inventory;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Island.Gameplay.Services.World.Objects.UI
{
    public class WorldCraftReceiptView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private InventoryCellView _result;
        [SerializeField] private TMP_Text _name;
        private readonly ISubject<CraftReceiptEntity> _onButtonClicked = new Subject<CraftReceiptEntity>();

        public IObservable<CraftReceiptEntity> OnButtonClicked => _onButtonClicked;

        public void Init(CraftReceiptEntity receiptEntity, InventoryConfig inventoryConfig)
        {
            _name.SetText(receiptEntity.Result.Type.ToString());
            _result.SetItem(inventoryConfig[receiptEntity.Result.Type], receiptEntity.Result.Count);
            _button.OnClickAsObservable().Subscribe(_ => _onButtonClicked.OnNext(receiptEntity)).AddTo(this);
        }
    }
}