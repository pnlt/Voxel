// Designed by KINEMATION, 2024.

using System;
using Demo.Scripts.Runtime.Character;
using InfimaGames.LowPolyShooterPack;
using System.Collections.Generic;
using Akila.FPSFramework;
using InfimaGames.LowPolyShooterPack._Project.ScriptsPN;
using KINEMATION.FPSAnimationFramework.Runtime.Layers.IkMotionLayer;
using Unity.Netcode;
using UnityEngine;

namespace Demo.Scripts.Runtime.Item
{
    public abstract class FPSItem : MonoBehaviour
    {
        public enum WeaponState
        {
            DROP,
            NONE
        }
        [SerializeField] protected RuntimeAnimatorController overrideController;
        
        [SerializeField] protected IkMotionLayerSettings equipMotion;
        [SerializeField] protected IkMotionLayerSettings unEquipMotion;

        private FPSPickable replacement;
        
        public List<FPSProjectiles> Projectiles { get; set; } = new List<FPSProjectiles>();
        
        public virtual void OnEquip(GameObject parent) { }
        
        public virtual void OnUnEquip() { }

        public virtual bool OnAimPressed() { return false; }

        public virtual bool OnAimReleased() { return false; }

        public virtual bool OnFirePressed() { return false; }

        public virtual bool OnFireReleased() { return false; }

        public virtual bool OnReload() { return false; }

        public virtual bool OnGrenadeThrow() { return false; }

        public virtual void OnCycleScope() { }

        public virtual void OnChangeFireMode() { }

        public virtual void OnAttachmentChanged(int attachmentTypeIndex) { }

        public void SetUp(FPSPickable pickableObj)
        {
            replacement = pickableObj;
        }

        public void Drop(Vector3 force, Vector3 torque, Transform dropLocation, FPSController inventory)
        {
            if (inventory.Items.Count > 0)
            {
                inventory.ChangeWeapon(WeaponState.DROP);
            }

            if (replacement == null)
            {
                inventory.ChangeWeapon(WeaponState.DROP);
                inventory.RemoveItem(this);
                Destroy(gameObject);
                return;
            }

            inventory.RemoveItem(this);
            FPSPickable pickUpObj = Instantiate(replacement, dropLocation.position, inventory.transform.rotation);
            if (pickUpObj.GetComponent<Rigidbody>())
            {
                pickUpObj.GetComponent<Rigidbody>().AddForce(force, ForceMode.VelocityChange);
                pickUpObj.GetComponent<Rigidbody>().AddTorque(torque, ForceMode.VelocityChange);
            }
            
            Destroy(gameObject);
        }
    }
}