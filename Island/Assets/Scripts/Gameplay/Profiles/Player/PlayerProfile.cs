using MemoryPack;
using TendedTarsier.Core.Services.Profile;

namespace Island.Gameplay.Profiles
{
    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class PlayerProfile : ProfileBase
    {
        public override string Name => "PlayerProfile";
        
        [MemoryPackOrder(0)]
        public int Level { get; set; }
        
    }
}