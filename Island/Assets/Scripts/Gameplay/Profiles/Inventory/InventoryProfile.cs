using System.Linq;
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
    public partial class InventoryProfile : ServerProfileBase
    {
        protected override string NetworkName => "Inventory";
        public ItemStack this[InventoryItemType type] => InventoryItems.FirstOrDefault(t => t.Type == type);

        [MemoryPackIgnore]
        public InventoryItemType SelectedItemType => InventoryItems[SelectedItem.Value].Type;

        [MemoryPackOrder(0)] 
        public ReactiveCollection<ItemStack> InventoryItems { get; [UsedImplicitly] set; } = new();

        [MemoryPackOrder(1), MemoryPackAllowSerialize]
        public ReactiveProperty<int> SelectedItem { get; [UsedImplicitly] set; } = new(0);

        private InventoryConfig _config;

        [Inject]
        private void Construct(InventoryConfig config)
        {
            _config = config;
        }

        public override void OnSectionCreated()
        {
            for (int i = 0; i < _config.InventoryCapacity; i++)
            {
                var item = new ItemStack();
                if (_config.DefaultItems.Count > i)
                {
                    item = _config.DefaultItems[i];
                }
                InventoryItems.Add(item);
            }
        }

        public override void RegisterFormatters()
        {
            base.RegisterFormatters();
            MemoryPackFormatterProvider.Register(new ReactivePropertyFormatter<InventoryItemType>());
            MemoryPackFormatterProvider.Register(new ReactiveCollectionFormatter<ItemStack>());
        }
    }
}