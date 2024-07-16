using UnityEngine;

namespace Upgrades
{
    public class WallReinforcement : PassiveUpgrade
    {
        public override UpgradeType Type => UpgradeType.WallReinforce;
        public override bool Activate()
        {
            ParentSegment.wallMaxHealth += 1;
            ParentSegment.wallHealth += 1;
            return true;
        }
    }
}
