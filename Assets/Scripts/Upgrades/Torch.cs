using UnityEngine;

namespace Upgrades
{
    public class Torch : PassiveUpgrade
    {
        public override UpgradeType Type => UpgradeType.Flame;
        public override bool Activate()
        {
            ParentSegment.soldier.Damage += 1;
            return true;
        }
    }
}
