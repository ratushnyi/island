using System;
using Island.Gameplay.Configs.Craft;
using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Panels.Player.Inventory;
using Island.Gameplay.Profiles.Inventory;
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

        public void Init(CraftReceiptEntity receiptEntity, InventoryConfig inventoryConfig, InventoryProfile inventoryProfile)
        {
            _name.SetText(receiptEntity.Result.ToString());
            _result.SetItem(inventoryProfile.InventoryItems, inventoryConfig[receiptEntity.Result], 1);
            _button.OnClickAsObservable().Subscribe(_ => _onButtonClicked.OnNext(receiptEntity)).AddTo(this);
        }
    }
}