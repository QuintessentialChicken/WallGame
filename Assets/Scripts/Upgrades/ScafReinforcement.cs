namespace Upgrades
{
    public class ScafReinforcement : PassiveUpgrade
    {
        // Update is called once per frame
        public override UpgradeType Type => UpgradeType.ScafReinforce;
        public override bool Activate()
        {
            ParentSegment.scaffoldingMaxHealth += 1;
            ParentSegment.RepairScaffolding();
            return true;
        }
    }
}
