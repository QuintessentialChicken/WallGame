using Player;
using UnityEngine;

namespace Interaction
{
    public class WallthersShed : MonoBehaviour, IInteractable
    {
        public void Interact(ThirdPersonController player)
        {
            print("Interacting");
            DayNightManager.instance.RequestChangeTo(DayNightManager.TimeOfDay.Day_Siege);
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }
    }
}
