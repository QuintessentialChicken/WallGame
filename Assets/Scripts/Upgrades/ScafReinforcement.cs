using UnityEngine;

namespace Upgrades
{
    public class ScafReinforcement : PassiveUpgrade
    {
        private void Start()
        {
            positionOffset = new Vector3(0, 0, 0.37f);
        }
        public override UpgradeType Type => UpgradeType.ScafReinforce;
        public override bool Activate()
        {
            ParentSegment.scaffoldingMaxHealth += 1;
            ParentSegment.RepairScaffolding();
            return true;
        }
    }
}
