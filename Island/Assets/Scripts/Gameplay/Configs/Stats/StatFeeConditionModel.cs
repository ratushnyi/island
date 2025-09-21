using System;
using UnityEngine;

namespace Island.Gameplay.Configs.Stats
{
    [Serializable]
    public class StatFeeConditionModel
    {
        public enum FeeConditionType
        {
            MinValue = 0,
            MaxValue = 1,
        }

        [field:SerializeField] 
        public StatType Type {get;set;}
        
        [field:SerializeField] 
        public FeeConditionType Condition {get;set;}
        
        [field:SerializeField] 
        public StatFeeModel FeeModel {get;set;}
    }
}