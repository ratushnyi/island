using Island.Gameplay.Services.Inventory.Items;
using TendedTarsier.Core.Utilities.Extensions;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.World.Objects
{
    public abstract class WorldContainerBase : WorldObjectBase
    {
        [Inject] private WorldService _worldService;
        protected NativeArray<ItemEntity>.ReadOnly Container => _container.AsNativeArray();
        private readonly NetworkList<ItemEntity> _container = new();
        
        public void InitContainer(ItemEntity[] container)
        {
            foreach (var item in container)
            {
                _container.Add(item);
            }
        }
        
        protected void ApplyContainer(ItemEntity[] stack)
        {
            foreach (var item in stack)
            {
                ItemEntity resultItem;
                var index = _container.IndexOf(t => t.Type == item.Type);
                if (index == -1)
                {
                    if (item.Count >= 0)
                    {
                        resultItem = item;
                        _container.Add(item);
                    }
                    else if (item.Count == 0)
                    {
                        Debug.LogError($"Trying to add empty item {item.Type} in container {name}");
                        return;
                    }
                    else
                    {
                        Debug.LogError($"Trying to remove item {item.Type} not contained in container {name}");
                        return;
                    }
                }
                else
                {
                    var result = _container[index].Count + item.Count;
                    if (result >= 0)
                    {
                        resultItem = new ItemEntity(item.Type, result);
                        _container[index] = resultItem;
                    }
                    else
                    {
                        Debug.LogError($"Trying to remove item {item.Type} more ({item.Count}) than contained ({_container[index].Count}) in container {name}");
                        return;
                    }
                }

                _worldService.UpdateContainer(this, resultItem);
            }
        }
    }
}