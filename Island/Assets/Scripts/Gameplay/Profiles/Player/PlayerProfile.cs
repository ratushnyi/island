using MemoryPack;

namespace Island.Gameplay.Profiles
{
    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class PlayerProfile : NetworkProfileBase
    {
        protected override string NetworkName => "PlayerProfile";
        
        [MemoryPackOrder(0)]
        public int Level { get; set; }
    }
}