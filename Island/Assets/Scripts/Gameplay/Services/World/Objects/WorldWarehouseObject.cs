using Cysharp.Threading.Tasks;
using Island.Gameplay.Services.Inventory;
using Island.Gameplay.Services.World.Objects.UI;
using TendedTarsier.Core.Panels;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.World.Objects
{
    public class WorldWarehouseObject : WorldContainerBase, ISelfPerformable
    {
        [SerializeField] private string _name = "Warehouse";
        [Inject] private PanelLoader<WorldWarehousePopup> _popup;
        public override string Name => _name;

        public override async UniTask<bool> Perform(bool isJustUsed, float deltaTime)
        {
            if (!isJustUsed || IsBusy)
            {
                return false;
            }

            SetBusy_ServerRpc(true);
            var popup = await _popup.Show(extraArgs: new object[] { this });
            await popup.WaitForHide();

            SetBusy_ServerRpc(false);

            return true;
        }
    }
}