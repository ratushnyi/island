using Island.Gameplay.Configs.Craft;
using Island.Gameplay.Configs.Inventory;
using TendedTarsier.Core.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Island.Gameplay.Services.World.Items
{
    public class CraftPopup : ResultPopupBase<CraftReceipt>
    {
        [SerializeField] private Button _buttonPlus;
        [SerializeField] private Button _buttonMinus;
        [SerializeField] private Button _buttonCraft;
        [SerializeField] private TMP_InputField _countField;
        [SerializeField] private Transform _receiptsContainer;
        [SerializeField] private Transform _ingredientsContainer;

        [Inject] private InventoryConfig _inventoryConfig;
        [Inject] private CraftConfig _craftConfig;
    }
}