using System;
using MemoryPack;
using TendedTarsier.Core.Services.Profile;
using UniRx;

namespace Island.Gameplay.Profiles
{
    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class DateTimeProfile : ProfileBase
    {
        public override string Name => "DateTime";

        [MemoryPackOrder(0), MemoryPackAllowSerialize]
        public ReactiveProperty<float> Minutes { get; set; } = new();
    }
}