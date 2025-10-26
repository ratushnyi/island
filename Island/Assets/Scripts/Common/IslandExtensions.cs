using System;
using Island.Gameplay.Services.World;
using Island.Gameplay.Services.World.Items;
using UnityEngine;

namespace Island.Common
{
    public static class IslandExtensions
    {
        public static int GenerateHash(this WorldObjectBase objectBase)
        {
            return GenerateHash(objectBase.Type, objectBase.transform.position, objectBase.transform.rotation);
        }

        public static int GenerateHash(WorldObjectType type, Vector3 position, Quaternion rotation = default)
        {
            return HashCode.Combine(type, position, rotation);
        }
    }
}