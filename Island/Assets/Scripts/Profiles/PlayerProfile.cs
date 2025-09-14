using System;
using MemoryPack;
using TendedTarsier.Core.Services.Profile;

namespace Module
{
    public class PlayerProfile : ProfileBase
    {
        public override string Name { get; } = "PlayerProfile";
        
        [MemoryPackOrder(0)]
        public int Level { get; set; }

        [MemoryPackOrder(1)]
        public int Energy { get; set; }

        public override void OnSectionCreated()
        {
            Energy = 100;
        }
        
    }
}