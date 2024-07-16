namespace Upgrades
{
    public class BrokenMirror : PassiveUpgrade
    {
        // Start is called before the first frame update
        public override UpgradeType Type => UpgradeType.BrokenMirror;
        public override bool Activate()
        {
            ParentSegment.probabilityModifier += 0.5f;
            return true;
        }
    }
}
