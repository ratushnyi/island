using System;
using Island.Gameplay.Configs.Craft;
using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Panels.Inventory;
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
        private readonly ISubject<CraftReceipt> _onButtonClicked = new Subject<CraftReceipt>();

        public IObservable<CraftReceipt> OnButtonClicked => _onButtonClicked;

        public void Init(CraftReceipt receipt, InventoryConfig inventoryConfig)
        {
            _name.SetText(receipt.Result.Type.ToString());
            _result.SetItem(inventoryConfig[receipt.Result.Type], receipt.Result.Count);
            _button.OnClickAsObservable().Subscribe(_ => _onButtonClicked.OnNext(receipt)).AddTo(this);
        }
    }
}