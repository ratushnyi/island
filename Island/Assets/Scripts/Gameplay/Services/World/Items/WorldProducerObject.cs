using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Inventory.Producer;
using Island.Gameplay.Services.World.Items;
using UnityEngine;

namespace Island.Gameplay.Services.World.Producers
{
    public class WorldProducerObject : WorldObjectBase, IPerformable
    {
        [SerializeField] private ProducerItemEntity _entity;
        [SerializeField] private string _name;
        [SerializeField] private WorldObjectType _type;
        public ProducerItemEntity Entity => _entity;
        public override string Name => _name;
        public override WorldObjectType Type => _type;

        public UniTask<bool> Perform()
        {
            return _entity.Perform();
        }
    }
}