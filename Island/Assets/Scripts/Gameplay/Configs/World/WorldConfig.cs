using System.Collections.Generic;
using Island.Gameplay.Services.Inventory;
using TendedTarsier.Core.Services.Modules;
using UnityEngine;

namespace Island.Gameplay.Configs.World
{
    [CreateAssetMenu(menuName = "Island/WorldConfig", fileName = "WorldConfig")]
    public class WorldConfig : ConfigBase
    {
        public List<WorldItemObject> WorldItemObjects { get; private set; }
    }
}