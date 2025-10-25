using System.Collections.Generic;
using System.Linq;
using Island.Common.Services.Network;
using Island.Gameplay.Services.Inventory.Items;
using MemoryPack;
using TendedTarsier.Core.Services.Profile;
using Unity.Netcode;

namespace Island.Gameplay.Profiles
{
    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class WorldProfile : ProfileBase
    {
        public override string Name => "World";

        [MemoryPackOrder(0)] public List<NetworkSpawnRequest> WorldObjectPlaced { get; set; } = new();
        [MemoryPackOrder(1)] public HashSet<int> WorldObjectDestroyed { get; set; } = new();
        [MemoryPackOrder(2)] public Dictionary<int, int> ObjectHealthDictionary { get; set; } = new();
        [MemoryPackOrder(3)] public Dictionary<int, ObjectContainer> ObjectContainerDictionary { get; set; } = new();
    }
    
    [MemoryPackable]
    public partial class ObjectContainer
    {
        [MemoryPackAllowSerialize]
        public Dictionary<InventoryItemType, int> Items = new();

        public List<ItemEntity> ToItemsList()
        {
            return Items.Select(item => new ItemEntity(item.Key, item.Value)).ToList();
        }
    }
}