using TendedTarsier.Core.Panels;
using UnityEngine;

namespace Island.Gameplay.Panels.HUD
{
    public class InputPanel : PanelBase
    {
        public override bool ShowInstantly => Application.isMobilePlatform;
    }
}