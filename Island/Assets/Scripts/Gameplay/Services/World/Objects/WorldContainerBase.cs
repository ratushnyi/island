using System;
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
        private readonly NetworkList<ItemStack> _container = new();
        
        public NativeArray<ItemStack>.ReadOnly ContainerArray => _container.AsNativeArray();
        public IObservable<NetworkListEvent<ItemStack>> OnContainerChanged => _container.AsObservable();

        public void InitContainer(ItemStack[] container)
        {
            foreach (var item in container)
            {
                _container.Add(item);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ApplyContainer_ServerRpc(ItemStack[] stack)
        {
            foreach (var item in stack)
            {
                ItemStack resultItem;
                var index = _container.IndexOf(t => t.Type == item.Type);
                if (index == -1)
                {
                    switch (item.Count)
                    {
                        case > 0:
                            resultItem = item;
                            _container.Add(item);
                            break;
                        case 0:
                            Debug.LogError($"Trying to add empty item {item.Type} in container {name}");
                            return;
                        default:
                            Debug.LogError($"Trying to remove item {item.Type} not contained in container {name}");
                            return;
                    }
                }
                else
                {
                    var result = _container[index].Count + item.Count;
                    switch (result)
                    {
                        case > 0:
                            resultItem = new ItemStack(item.Type, result);
                            _container[index] = resultItem;
                            break;
                        case 0:
                            resultItem = new ItemStack(item.Type, 0);
                            _container.RemoveAt(index);
                            break;
                        default:
                            Debug.LogError($"Trying to remove item {item.Type} more ({item.Count}) than contained ({_container[index].Count}) in container {name}");
                            return;
                    }
                }

                _worldService.UpdateContainer(this, resultItem);
            }
        }
    }
}