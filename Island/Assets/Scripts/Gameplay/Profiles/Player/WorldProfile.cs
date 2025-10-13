using System.Collections.Generic;
using Island.Common.Services.Network;
using MemoryPack;
using TendedTarsier.Core.Services.Profile;

namespace Island.Gameplay.Profiles
{
    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class WorldProfile : ProfileBase
    {
        public override string Name => "World";

        [MemoryPackOrder(0)] public List<NetworkSpawnRequest> WorldItemPlaced { get; set; } = new();

        [MemoryPackOrder(1)] public HashSet<int> WorldItemRemoved { get; set; } = new();
    }
}