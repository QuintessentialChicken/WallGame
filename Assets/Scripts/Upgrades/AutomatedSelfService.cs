using UnityEngine;

namespace Upgrades
{
    public class AutomatedSelfService : CooldownUpgrade
    {
        public override void Engage()
        {
            Debug.LogWarning("I Can't repair myself yet ;(");
        }
        public override UpgradeType Type => UpgradeType.AutomatedSelfService;
    }
}