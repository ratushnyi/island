using Island.Gameplay.Configs.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using JetBrains.Annotations;
using MemoryPack;
using TendedTarsier.Core.Utilities.MemoryPack.FormatterProviders;
using UniRx;
using Zenject;

namespace Island.Gameplay.Profiles.Inventory
{
    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class InventoryProfile : NetworkProfileBase
    {
        protected override string NetworkName => "Inventory";

        [MemoryPackOrder(0)] 
        public ReactiveDictionary<InventoryItemType, ReactiveProperty<int>> InventoryItems { get; [UsedImplicitly] set; } = new();

        [MemoryPackOrder(1), MemoryPackAllowSerialize]
        public ReactiveProperty<InventoryItemType> SelectedItem { get; [UsedImplicitly] set; } = new(default);

        private InventoryConfig _config;

        [Inject]
        private void Construct(InventoryConfig config)
        {
            _config = config;
        }

        public override void OnSectionCreated()
        {
            foreach (var item in _config.DefaultItems)
            {
                InventoryItems.Add(item.Type, new ReactiveProperty<int>(item.Count));
            }
        }

        public override void RegisterFormatters()
        {
            base.RegisterFormatters();
            MemoryPackFormatterProvider.Register(new ReactivePropertyFormatter<InventoryItemType>());
            MemoryPackFormatterProvider.Register(new ReactiveDictionaryFormatter<InventoryItemType, ReactiveProperty<int>>());
        }
    }
}