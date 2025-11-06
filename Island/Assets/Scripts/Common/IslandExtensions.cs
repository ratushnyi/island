using System;
using Island.Gameplay.Services.World.Objects;
using UnityEngine;

namespace Island.Common
{
    public static class IslandExtensions
    {
        public static int AimHash = -1;
        public static int GroundHash = 0;

        public static int[] SystemHashes =
        {
            AimHash,
            GroundHash
        };

        public static int GenerateHash(this WorldObjectBase objectBase)
        {
            return GenerateHash(objectBase.transform.position, objectBase.transform.rotation);
        }

        public static int GenerateHash(Vector3 position, Quaternion rotation = default)
        {
            return HashCode.Combine(position, rotation);
        }
    }
}