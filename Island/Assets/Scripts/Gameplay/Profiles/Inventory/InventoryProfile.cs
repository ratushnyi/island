using JetBrains.Annotations;
using MemoryPack;
using TendedTarsier.Core.Services.Profile;
using UniRx;

namespace Island.Gameplay.Profiles.Inventory
{
    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class InventoryProfile : ProfileBase
    {
        public override string Name => "Inventory";

        [MemoryPackOrder(0)]
        public ReactiveDictionary<string, ReactiveProperty<int>> InventoryItems { get; [UsedImplicitly] set; } = new();

        [MemoryPackOrder(1), MemoryPackAllowSerialize]
        public ReactiveProperty<string> SelectedItem { get; [UsedImplicitly] set; } = new(string.Empty);
    }
}