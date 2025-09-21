using System.Collections.Generic;
using Island.Gameplay.Configs.Stats;
using JetBrains.Annotations;
using MemoryPack;
using TendedTarsier.Core.Services.Profile;

namespace Island.Gameplay.Profiles.Stats
{
    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class StatsProfile : ProfileBase
    {
        public override string Name => "Stats";

        [MemoryPackOrder(0)]
        public Dictionary<StatType, StatProfileElement> StatsDictionary { get; [UsedImplicitly] set; } = new();
    }
}