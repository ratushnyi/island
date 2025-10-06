using System.Collections.Generic;
using Island.Gameplay.Configs.Stats;
using JetBrains.Annotations;
using MemoryPack;

namespace Island.Gameplay.Profiles.Stats
{
    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class StatsProfile : NetworkProfileBase
    {
        protected override string NetworkName => "Stats";

        [MemoryPackOrder(0)]
        public Dictionary<StatType, StatProfileElement> StatsDictionary { get; [UsedImplicitly] set; } = new();
    }
}