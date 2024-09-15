using System.Collections;
using System.Collections.Generic;
using Demo.Scripts.Runtime.Character;
using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Player/Interactables Manager")]
    public class InteractablesManager : MonoBehaviour
    {
        [Tooltip("The allowed range for any interaction")]
        public float range = 2;
        [Tooltip("If 1 player interaction angle is 360 if 0.5 interaction angle is 180")]
        public float fieldOfInteractions = 0.5f;
        [Tooltip("What layer to interact with")]
        public LayerMask interactableLayers = -1;
        [Tooltip("The UI Object which contains all the data about the interaction")]
        public GameObject HUDObject;
        [Tooltip("The display text for interact key")]
        public TextMeshProUGUI interactKeyText;
        [Tooltip("The interaction name which will show if in range EXAMPLES (Open, Pickup, etc..)")]
        public TextMeshProUGUI interactActionText;
        public AudioProfile defaultInteractAudio;

        public Audio interactAudio = new Audio();
        private IInteractable interactable;
        public FPSController inventory;

        public bool IsPickUp
        {
            get
            {
                return IsPickUp;
            }
        }

        private void Start()
        {
            inventory = GetComponent<FPSController>();
            interactAudio.Equip(gameObject, defaultInteractAudio);
        }

        private void Update()
        {
            interactable = GetInteractable();

            if(HUDObject)
                HUDObject.SetActive(interactable != null);

            if(interactable != null)
            {
                if (interactKeyText) interactKeyText.SetText("T");
                if(interactActionText) interactActionText.SetText(interactable.GetInteractionName());
            }
        }

        public void OnPickUp()
        {
            interactable.Interact(this);
        }

        public IInteractable GetInteractable()
        {
            List<IInteractable> interactables = new List<IInteractable>();

            Collider[] colliders = Physics.OverlapSphere(transform.position, range, interactableLayers);
            foreach (Collider collider in colliders)
            {
                if (collider.TryGetComponent(out IInteractable interactable))
                {
                    interactables.Add(interactable);
                }
            }

            IInteractable closestInteractable = null;
            foreach(IInteractable interactable in interactables)
            {
                Vector3 position = transform.position;
                Vector3 interactablePosition = interactable.transform.position;


                if (closestInteractable == null)
                {
                    closestInteractable = interactable;
                }
                else if(Vector3.Distance(position, interactablePosition) < Vector3.Distance(position, closestInteractable.transform.position))
                {
                    closestInteractable = interactable;
                }

                var dir = (closestInteractable.transform.position - position).normalized;
                if (Vector3.Dot(transform.forward, dir) <= fieldOfInteractions)
                {
                    closestInteractable = null;
                }
            }

            return closestInteractable;
        }
    }
}