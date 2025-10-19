using Island.Gameplay.Services.Inventory.Producer;
using Island.Gameplay.Services.World.Items;
using UnityEngine;

namespace Island.Gameplay.Services.World.Producers
{
    public class WorldProducerObject : WorldObjectBase
    {
        [SerializeField] private ProducerItemEntity _entity;
        [SerializeField] private string _name;
        [SerializeField] private WorldObjectType _type;
        public ProducerItemEntity Entity => _entity;
        public override string Name => _name;
        public override WorldObjectType Type => _type;
    }
}