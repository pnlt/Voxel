using System.Collections;
using System.Collections.Generic;
using Akila.FPSFramework;
using Demo.Scripts.Runtime.Character;
using Demo.Scripts.Runtime.Item;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    public enum FPSPickableType
    {
        ITEM
    }

    public class FPSPickable : MonoBehaviour, IInteractable
    {
        
        public FPSPickableType type;
        public FPSItem item;
        public AudioProfile interactSound;
        public string interactionName = "Take";
        public string displayName = "Item";

        private FirearmAttachmentsManager firearmAttachmentsManager;

        private void Start()
        {
            firearmAttachmentsManager = transform.SearchFor<FirearmAttachmentsManager>();
        }

        /// <summary>
        /// Perform interaction from the interface.
        /// </summary>
        /// <param name="source"></param>
        public void Interact(InteractablesManager source)
        {
            if (interactSound)
                source.interactAudio?.PlayOneShot(interactSound);

            if (type == FPSPickableType.ITEM)
                //can Pickup
                InteractWithItem(source);

                //Play pickup animation
                /*if (source.inventory.Items.ToArray().Length != 0 &&
                    source.inventory.Items[source.inventory.currentItemIndex].Animator)
                {
                    source.Inventory.items[source.Inventory.currentItemIndex].Animator.CrossFade("Pickup", 0.1f, 0, 0);
                }*/
        }

        public string GetInteractionName()
        {
            string info = "";
            if (type == FPSPickableType.ITEM)
                info = $"{displayName} - {type}";


            return $"{interactionName} {info}";
        }

        /// <summary>
        /// Performs the interaction with the item
        /// </summary>
        /// <param name="source"></param>
        private void InteractWithItem(InteractablesManager source)
        {
            var inventory = source.inventory;
            StartCoroutine(PerformItemPickup(inventory));
        }


        private IEnumerator PerformItemPickup(FPSController inventory)
        {
            yield return null;
            FPSItem newItem = null;
            //Case2: your picked-up weapon is the last slot in ur inventory
            if (inventory.Items.Count < inventory.MaxSlot)
            {
                inventory.AddItem(item);
            }
            //Case1: When your picked-up weapon make ur inventory oversize
            //Remove the current active weapon and replace it with the pick-up weapon
            else if (inventory.Items.Count >= inventory.MaxSlot)
            {
                FPSItem oldItem = inventory.GetActiveItem();
                //inventory.ReplaceItem(oldItem, item);
            }
        
            /*if (newItem.TryGetComponent<Firearm>(out Firearm firearm))
            {
                firearm.attachmentsManager.CopyFrom(firearmAttachmentsManager);
        
                yield return new WaitForFixedUpdate();
                firearm.remainingAmmoCount = ammoAmount;
            }*/
        
            Destroy(gameObject);
        }
    }
}
