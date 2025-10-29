using Island.Gameplay.Panels.Inventory;
using TMPro;
using UnityEngine;

namespace Island.Gameplay.Services.World.Items
{
    public class CraftReceiptView : MonoBehaviour
    {
        [SerializeField] private InventoryCellView _result;
        [SerializeField] private TMP_Text _name;
    }
}