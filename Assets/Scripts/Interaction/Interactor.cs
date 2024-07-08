using System.Collections.Generic;
using Input;
using Player;
using UnityEngine;

namespace Interaction
{
    internal interface IInteractable
    {
        public void Interact(ThirdPersonController player);
        Vector3 GetPosition();
    }

    internal delegate void Interaction(ThirdPersonController player);

    [RequireComponent(typeof(ThirdPersonController))]
    public class Interactor : MonoBehaviour
    {
        private ThirdPersonController _controller;

        private List<IInteractable> _interactables;

        private void Awake()
        {
            _interactables = new List<IInteractable>();
            _controller = GetComponent<ThirdPersonController>();

            Inputs.Interact += Interact;
        }

        private void OnDestroy()
        {
            Inputs.Interact -= Interact;
        }
        // Start is called before the first frame update

        private void OnTriggerEnter(Collider other)
        {
            var interactable = other.gameObject.GetComponent<IInteractable>();

            if (interactable != null) _interactables.Add(interactable);

            /*
             * Possibly call to UI class to display Tooltip (Press E to Interact)
             */
            for (var i = 0; i < _interactables.Count; i++)
            {
                var test = _interactables[i];
                print("entered " + i + " " + test);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            _interactables.Remove(other.gameObject.GetComponent<IInteractable>());
            for (var i = 0; i < _interactables.Count; i++)
            {
                var interactable = _interactables[i];
                print("exited " + i + " " + interactable);
            }
        }

        private void Interact()
        {
            if (_interactables.Count == 0) return;

            IInteractable closestInteractable = default;
            var closestDistance = float.MaxValue;

            foreach (var interactable in _interactables)
            {
                var distance = Vector3.Distance(transform.position, interactable.GetPosition());
                if (!(distance < closestDistance)) continue;
                closestDistance = distance;
                closestInteractable = interactable;
            }

            closestInteractable?.Interact(_controller);
        }
    }
}