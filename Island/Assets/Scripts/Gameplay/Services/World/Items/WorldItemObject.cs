using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Inventory.Items;
using Island.Gameplay.Services.Inventory.Tools;
using Island.Gameplay.Services.Stats;
using TendedTarsier.Core.Services.Input;
using UniRx;
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

        [SerializeField] private List<ItemEntity> _materialsItems;
        [SerializeField] private ItemEntity _resultItem;
        [SerializeField] private WorldObjectType _type;
        [SerializeField] private WorldObjectInteractType _interactType;
        [SerializeField] private float _duration;

        [Inject] private InventoryService _inventoryService;
        [Inject] private InputService _inputService;
        [Inject] private AimService _aimService;
        [Inject] private StatsService _statsService;

        private UniTask<int> HoldAwaiter;
        private UniTask DelayAwaiter;
        public override WorldObjectType Type => _type;
        public override string Name => _type.ToString();

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

        public async UniTask<bool> Perform(ToolItemType toolType)
        {
            _tools.TryGetValue(ToolItemType.None, out var damage);
            _tools.TryGetValue(toolType, out damage);

            if (await Perform())
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

        private async UniTask<bool> Perform()
        {
            if (HoldAwaiter.Status == UniTaskStatus.Pending || DelayAwaiter.Status == UniTaskStatus.Pending)
            {
                return false;
            }
            
            if (Health.Value <= 0)
            {
                return false;
            }
            
            if (!Check())
            {
                return false;
            }
            
            if (!await Pay())
            {
                return false;
            }

            await Collect();

            return true;
        }

        private bool Check()
        {
            foreach (var material in _materialsItems)
            {
                if (!_inventoryService.IsSuitable(material.Type, material.Count))
                {
                    return false;
                }
            }

            if (!_inventoryService.IsEnoughSpace(_resultItem))
            {
                return false;
            }
            
            return true;
        }

        private async UniTask<bool> Pay()
        {
            foreach (var material in _materialsItems)
            {
                _inventoryService.TryRemove(material.Type, material.Count);
            }
            
            var result = true;
            if (_interactType == WorldObjectInteractType.Hold)
            {
                result = await performHold();
            }

            return result;

            async UniTask<bool> performHold()
            {
                var onPlayerExit = _aimService.TargetObject.SkipLatestValueOnSubscribe().First().ToUniTask();
                var onStopInteract = _inputService.OnInteractButtonCanceled.First().ToUniTask();
                var progressBar = UniTask.Delay(TimeSpan.FromSeconds(_duration));

                HoldAwaiter = UniTask.WhenAny(progressBar, onStopInteract, onPlayerExit);

                return await HoldAwaiter == 0;
            }
        }

        private async UniTask Collect()
        {
            if (_interactType == WorldObjectInteractType.Await)
            {
                await performAwait();
            }
            
            _inventoryService.TryCollect(_resultItem);

            async UniTask performAwait()
            {
                DelayAwaiter = UniTask.Delay(TimeSpan.FromSeconds(_duration));
                await DelayAwaiter;
            }
        }
    }
}