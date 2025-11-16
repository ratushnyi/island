using Island.Gameplay.Settings;
using MemoryPack;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Profiles
{
    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class PlayerProfile : ServerProfileBase
    {
        protected override string NetworkName => "PlayerProfile";
        
        [MemoryPackOrder(0)]
        public Vector3 Position { get; set; }
        
        [MemoryPackOrder(1)]
        public Quaternion Rotation { get; set; }

        [Inject] private PlayerConfig _playerConfig;

        public override void OnSectionCreated()
        {
            Position = _playerConfig.SpawnPosition;
        }
    }
}