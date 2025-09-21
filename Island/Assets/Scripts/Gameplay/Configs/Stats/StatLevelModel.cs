using System;
using UnityEngine;

namespace Island.Gameplay.Configs.Stats
{
    [Serializable]
    public class StatLevelModel
    {
        [field: SerializeField]
        public int NextLevelExperience { get; private set; }

        [field: SerializeField]
        public int MaxValue { get; private set; }

        [field: SerializeField]
        public int DefaultValue { get; private set; }

        [field: SerializeField]
        public int RecoveryRate { get; private set; }

        [field: SerializeField]
        public int RecoveryValue { get; private set; }
    }
}