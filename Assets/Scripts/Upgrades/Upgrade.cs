using UnityEngine;
using Wall;

namespace Upgrades
{
    
    
    public abstract class Upgrade : MonoBehaviour
    {
        public WallSegment ParentSegment { get; set; }

        public enum UpgradeType
        {
            None,
            AutomatedSelfService,
            LeviatedSpringDefense
        }

        public abstract UpgradeType Type
        {
            get;
        }

        /* Activates the Upgrade if possible and returns false if not possible, true otherwise. */
        public abstract bool Activate();

        public abstract void UpgradeUpdate();

        public UpgradeName UpgradeName;

        public bool DEBUG_Activate = false;


        private void Update()
        {
            if (DEBUG_Activate)
            {
                var success = Activate();
                if (success)
                    Debug.Log("Activation Successful.");
                else
                    Debug.Log("Activation Failed!");
                DEBUG_Activate = false;
            }

            UpgradeUpdate();
        }
    }
}