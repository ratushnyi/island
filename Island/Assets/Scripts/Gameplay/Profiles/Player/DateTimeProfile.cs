using System;
using MemoryPack;
using TendedTarsier.Core.Services.Profile;
using TendedTarsier.Core.Utilities.MemoryPack.FormatterProviders;
using UniRx;

namespace Island.Gameplay.Profiles
{
    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class DateTimeProfile : ProfileBase
    {
        public override string Name => "DateTime";

        [MemoryPackOrder(0), MemoryPackAllowSerialize]
        public ReactiveProperty<uint> Minutes { get; set; } = new();

        public DateTime GetDateTime()
        {
            DateTime startDate = new DateTime(1998, 5, 3);
            DateTime currentDate = startDate.AddMinutes(Minutes.Value);
            return currentDate;
        }

        public override void RegisterFormatters()
        {
            base.RegisterFormatters();
            MemoryPackFormatterProvider.Register(new ReactivePropertyFormatter<uint>());
        }
    }
}