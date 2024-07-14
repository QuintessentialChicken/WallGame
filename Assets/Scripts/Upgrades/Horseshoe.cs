using UnityEngine;

namespace Upgrades
{
    public class Horseshoe : PassiveUpgrade
    {
        public override UpgradeType Type => UpgradeType.Horseshoe;
        public override bool Activate()
        {
            ParentSegment.probabilityModifier -= 0.5f;
            return true;
        }
    }
}
