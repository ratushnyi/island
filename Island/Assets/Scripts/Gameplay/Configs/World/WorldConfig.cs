using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Island.Gameplay.Services.World.Items;
using TendedTarsier.Core.Services.Modules;
using UnityEngine;

namespace Island.Gameplay.Configs.World
{
    [CreateAssetMenu(menuName = "Island/WorldConfig", fileName = "WorldConfig")]
    public class WorldConfig : ConfigBase
    {
        [field: SerializedDictionary("Type", "Prefab")]
        public SerializedDictionary<WorldItemType, WorldItemObject> WorldItemObjects;
        [field: SerializeField]
        public List<WorldItemObject> WorldItemPlacement;
    }
}