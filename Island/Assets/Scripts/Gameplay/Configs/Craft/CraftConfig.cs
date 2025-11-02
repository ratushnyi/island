using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Island.Gameplay.Services.World.Objects;
using Island.Gameplay.Services.World.Objects.UI;
using TendedTarsier.Core.Services.Modules;
using UnityEngine;

namespace Island.Gameplay.Configs.Craft
{
    [CreateAssetMenu(menuName = "Island/CraftConfig", fileName = "CraftConfig")]
    public class CraftConfig : ConfigBase
    {
        [field: SerializedDictionary("Type", "Receipts")]
        public SerializedDictionary<WorldObjectType, List<CraftReceipt>> Receipts;

        [field: SerializeField]
        public WorldCraftReceiptView WorldCraftReceiptView { get; set; }
    }
}