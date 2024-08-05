using System.Collections.Generic;
using UnityEngine;
using Upgrades;

namespace Wall
{
    public class UpgradesStore : MonoBehaviour
    {
        public static UpgradesStore instance;

        // Dictionary to store upgrades for each segment
        private Dictionary<int, List<Upgrade>> segmentUpgrades = new();

        private void Awake()
        {
            // Ensure this object persists across scene loads
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SaveSegmentUpgrade(int segmentID, Upgrade upgrade)
        {
            if (segmentUpgrades.ContainsKey(segmentID))
            {
                segmentUpgrades[segmentID].Add(upgrade);
            }
            else
            {
                segmentUpgrades.Add(segmentID, new List<Upgrade> {upgrade});
            }
        }

        public List<Upgrade> GetSegmentUpgrades(int segmentID)
        {
            return segmentUpgrades.TryGetValue(segmentID, out var upgrades) ? upgrades : new List<Upgrade>();
        }
    }
}