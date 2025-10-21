using System;
using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.Inventory.Producer;
using Island.Gameplay.Services.World.Items;
using TendedTarsier.Core.Services.Input;
using UniRx;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.World.Producers
{
    public class WorldProducerObject : WorldObjectBase, IPerformable
    {
        [SerializeField] private ProducerItemEntity _entity;
        [SerializeField] private string _name;
        [SerializeField] private WorldObjectType _type;
        [SerializeField] private WorldObjectInteractType _interactType;
        [Inject] private InputService _inputService;
        [Inject] private AimService _aimService;
        public ProducerItemEntity Entity => _entity;
        public override string Name => _name;
        public override WorldObjectType Type => _type;

        public async UniTask<bool> Perform()
        {
            if (!await Check())
            {
                return false;
            }

            await Pay();

            return await _entity.Perform();
        }

        private async UniTask<bool> Check()
        {
            var result = _entity.IsEnoughResources();
            if (result && _interactType == WorldObjectInteractType.Hold)
            {
                result = await performHold();
            }

            return result;

            async UniTask<bool> performHold()
            {
                var onPlayerExit = _aimService.TargetObject.SkipLatestValueOnSubscribe().First().ToUniTask();
                var onStopInteract = _inputService.OnInteractButtonCanceled.First().ToUniTask();
                var progressBar = UniTask.Delay(TimeSpan.FromSeconds(_entity.Duration));

                return await UniTask.WhenAny(progressBar, onStopInteract, onPlayerExit) == 0;
            }
        }

        private async UniTask Pay()
        {
            _entity.Pay();
            if (_interactType == WorldObjectInteractType.Await)
            {
                await performAwait();
            }

            async UniTask performAwait()
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_entity.Duration));
            }
        }
    }
}