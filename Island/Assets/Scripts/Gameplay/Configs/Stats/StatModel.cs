using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Island.Gameplay.Configs.Stats
{
    [Serializable]
    public class StatModel
    {
        public StatLevelModel this[int level] => _levels.Count > level ? _levels[level] : _levels.Last();

        [field: SerializeField]
        public StatType StatType { get; private set; }

        [field: SerializeField]
        public Sprite Sprite { get; private set; }

        [field: SerializeField]
        public bool StatBar { get; private set; }

        [SerializeField]
        private List<StatLevelModel> _levels;
    }
}