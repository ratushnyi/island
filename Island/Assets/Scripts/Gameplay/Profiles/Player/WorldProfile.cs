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

        [MemoryPackOrder(0)] public Dictionary<int, NetworkSpawnRequest> SpawnedObjects { get; set; } = new();
        [MemoryPackOrder(1)] public HashSet<int> DestroyedObject { get; set; } = new();
        [MemoryPackOrder(2)] public Dictionary<int, int> ObjectHealthDictionary { get; set; } = new();
        [MemoryPackOrder(3)] public Dictionary<int, ObjectContainer> ObjectContainerDictionary { get; set; } = new();
    }
}