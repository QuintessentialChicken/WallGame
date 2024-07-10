using UnityEngine;
using Wall;

namespace Upgrades
{
    
    
    public abstract class Upgrade : MonoBehaviour
    {
        public WallSegment ParentSegment { get; set; }

        public enum UpgradeType
        {
            None = 0,

            // Passive
            Horseshoe = 1,
            ScafReinforce = 2,
            WallReinforce = 3,
            BrokenMirror = 4,
            Flame = 5,
            Telescope = 6,

            // Active
            LSD = 10, // LeviatedSpringDefense
            ASS = 11 // AutomatedSustainSystem
        }

        public abstract UpgradeType Type
        {
            get;
        }

        /* Activates the Upgrade if possible and returns false if not possible, true otherwise. */
        public abstract bool Activate();

        public abstract void UpgradeUpdate();

        public UpgradeType type;

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