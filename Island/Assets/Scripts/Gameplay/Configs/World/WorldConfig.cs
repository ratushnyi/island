using System.Collections.Generic;
using Island.Gameplay.Services.Inventory;
using UnityEngine;

namespace Island.Gameplay.Configs.World
{
    [CreateAssetMenu(menuName = "Island/WorldConfig", fileName = "WorldConfig")]
    public class WorldConfig : ScriptableObject
    {
        public List<WorldItemObject> WorldItemObjects { get; private set; }
    }
}