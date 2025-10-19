using AYellowpaper.SerializedCollections;
using Island.Gameplay.Services.Inventory.Items;
using Island.Gameplay.Services.Inventory.Tools;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.World.Items
{
    public class WorldItemObject : WorldObjectBase
    {
        [SerializeField, SerializedDictionary("Tool", "Damage")]
        private SerializedDictionary<ToolItemType, int> _tools;
        [SerializeField, SerializedDictionary("Health", "ViewObject")]
        private SerializedDictionary<int, GameObject> _healthView;
        [SerializeField] private ItemEntity _itemEntity;
        [SerializeField] private WorldObjectType _type;
        [Inject] private WorldService _worldService;

        public override WorldObjectType Type => _type;
        public override string Name => _type.ToString();
        public ItemEntity ItemEntity => _itemEntity;

        protected override void UpdateView(int health)
        {
            bool selected = false;
            foreach (var view in _healthView)
            {
                if (!selected && health >= view.Key)
                {
                    selected = true;
                    view.Value.SetActive(true);
                    continue;
                }
                view.Value.SetActive(false);
            }
        }

        public bool TryDestroy(ToolItemType toolType)
        {
            if (_tools.TryGetValue(toolType, out var damage))
            {
                var newHealth = Health.Value - damage;
                OnHealthChanged_ServerRpc(newHealth);
                if (newHealth <= 0)
                {
                    Despawn_ServerRpc();
                    return true;
                }
            }
            
            return false;
        }
    }
}