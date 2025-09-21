using MemoryPack;
using UniRx;

namespace Island.Gameplay.Profiles.Stats
{
    [MemoryPackable]
    public partial class StatProfileElement
    {
        [MemoryPackAllowSerialize]
        public ReactiveProperty<int> Level = new();
        [MemoryPackAllowSerialize]
        public ReactiveProperty<int> Experience = new();
        [MemoryPackAllowSerialize]
        public ReactiveProperty<int> Value = new();
        [MemoryPackAllowSerialize]
        public ReactiveProperty<int> Range = new();
    }
}