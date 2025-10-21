using System;
using UnityEngine;

namespace Island.Gameplay.Configs.Stats
{
    [Serializable]
    public class StatFeeModel
    {
        [field: SerializeField]
        public StatType Type { get; set; }

        [field: SerializeField]
        public int Rate { get; set; }

        [field: SerializeField]
        public int Value { get; set; }

        public float Deposit { get; set; }
    }
}