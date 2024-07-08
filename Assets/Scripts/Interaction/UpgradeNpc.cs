using Player;
using UnityEngine;
using Upgrades;

namespace Interaction
{
    public class UpgradeNpc : MonoBehaviour, IInteractable
    {
        public GameObject upgradePrefab;
        public void Interact(ThirdPersonController player)
        {
            print("Assigning upgrade: " + upgradePrefab.GetComponent<Upgrade>().UpgradeName);
            player.SelectedUpgrade = upgradePrefab;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }
    }
}
