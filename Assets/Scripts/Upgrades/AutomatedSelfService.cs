namespace Upgrades
{
    public class AutomatedSelfService : CooldownUpgrade
    {
        private void Start()
        {
            EventManager.OnWallPieceHit += TryActivate;
        }

        private void OnDestroy()
        {
            EventManager.OnWallPieceHit -= TryActivate;
        }
        
        private void TryActivate(int index)
        {
            if (ParentSegment.index == index)
            {
                Activate();
            }
        }

        protected override void Ready()
        {
            Activate();
        }

        public override bool Engage()
        {
            return ParentSegment.RepairWall();
        }
        public override UpgradeType Type => UpgradeType.ASS;
    }
}