using Player;
using UnityEngine;

namespace Interaction
{
    public class ResourceReplenisher : MonoBehaviour, IInteractable
    {
        public enum Resource
        {
            Wood,
            Stone
        }

        [SerializeField] private int replenishAmount = 3;

        public Resource resource;

        public void Interact(ThirdPersonController interactor)
        {
            if (resource == Resource.Wood)
            {
                EventManager.RaiseOnReplenishWood(replenishAmount);
                AudioManager.instance.PlayOneShot(FMODEvents.instance.pickWood,this.transform.position);
            }
            else
            {
                EventManager.RaiseOnReplenishStone(replenishAmount);
                AudioManager.instance.PlayOneShot(FMODEvents.instance.pickStone,this.transform.position);
            }
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }
    }
}