using Akila.FPSFramework;
using InfimaGames.LowPolyShooterPack._Project.ScriptsPN;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    [CreateAssetMenu(fileName = "New Weapon", menuName = "FPSGame/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        //Basics
        public string Name;
        public FPSPickable replacement;
        public WeaponHUD canves;

        //Fire Fields
        public Demo.Scripts.Runtime.Item.Weapon.WeaponMethodology shootingMechanism;
        /*public Weapon.RechargingType rechargingType;
        public Weapon.FireMode fireMode = Weapon.FireMode.Auto;
        public Weapon.FireRateUnit fireRateUnit = Weapon.FireRateUnit.RoundPerMinute;*/
        public Vector3Direction casingDirection = Vector3Direction.right;

        public LayerMask hittableLayers = -1;

        public FPSProjectiles projectile;
        public GameObject casing;
        public GameObject defaultDecal;
        public Vector3Direction decalDirection;
        public float fireRate = 833;
        public float muzzleVelocity = 250;
        public float ejectionVelocity = 10;
        public float impactForce = 10;
        public float damage = 20;
        public float range = 300;
        public AnimationCurve damageRangeCurve = new AnimationCurve(new Keyframe[] {new Keyframe(0, 1), new Keyframe(1, 0)});

        public float fireTransition = 0.1f;
        //public string rechargingStateName = "Recharging";
        public bool allowFireWhileRecharging;
        public float casingRotationFactor = 25;
        public float lowerdDelay = 10;

        public bool tracerRounds = true;

        public float decalSize = 1;

        public float afterShotReliefTime = 0.2f;
        public int shotCount = 1;
        public float shotDelay = 0;
        public bool alwaysApplyFire = false;

        /*public float gamepadVibrationAmountRight = 1;
        public float gamepadVibrationAmountLeft = 1;
        public float gamepadVibrationDuration = 0.1f;*/

        //Ammunition
        public AmmoProfileData ammoType;
        public int magazineCapacity = 30;
        public int reserve = 0;
        public bool automaticReload = true;

        public int amount = 0;
        //public Weapon.ReloadType reloadMethod;
        /*public float reloadTime = 1.6f;
        public float emptyReloadTime = 2.13f;

        public string reloadStateName = "Reload";
        public float reloadTransitionTime = 0.3f;*/


        //ADS (Aim Down Sight)
        public float aimSpeed = 10;
        public float aimFieldOfView = 50;
        public float aimWeaponFieldOfview = 40;


        //Movement
        /*public float Movement_WalkSpeed = 5;
        public float Movement_RunSpeed = 10;
        public float Movement_TacticalSprintSpeed = 12f;
        public float Movement_AimWalkSpeed = 3;
        public float Movement_FireWalkSpeed = 4;
        public bool allowFullMovementSpeedWhileAiming;*/


        //Audio
        public AudioProfile fire;
        public AudioProfile fireTail;
        public AudioProfile reloadAudio;
        public AudioProfile reloadEmptyAudio;

        public bool useFire { get; set; } = true;
        public bool useRecoil { get; set; } = true;
        public bool useMovement { get; set; } = true;
        public bool useAudio { get; set; } = true;
    }
}
