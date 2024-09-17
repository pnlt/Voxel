// Designed by KINEMATION, 2024.

using System;
using KINEMATION.FPSAnimationFramework.Runtime.Camera;
using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.FPSAnimationFramework.Runtime.Playables;
using KINEMATION.FPSAnimationFramework.Runtime.Recoil;
using KINEMATION.KAnimationCore.Runtime.Input;

using Demo.Scripts.Runtime.AttachmentSystem;

using System.Collections.Generic;
using Akila.FPSFramework;
using Demo.Scripts.Runtime.Character;
using InfimaGames.LowPolyShooterPack;
using InfimaGames.LowPolyShooterPack._Project.ScriptsPN;
using Unity.Netcode;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using MathUtilities = Akila.FPSFramework.MathUtilities;
using Random = UnityEngine.Random;

namespace Demo.Scripts.Runtime.Item
{
    public class Weapon : FPSItem
    {
        public enum WeaponMethodology
        {
            PROJECTILE,
            HITSCAN
        }
        [Header("General")]
        [SerializeField] [Range(0f, 120f)] private float fieldOfView = 90f;
        [SerializeField] private FPSAnimationAsset reloadClip;
        [SerializeField] private FPSCameraAnimation cameraReloadAnimation;
        
        [SerializeField] private FPSAnimationAsset grenadeClip;
        [SerializeField] private FPSCameraAnimation cameraGrenadeAnimation;

        #region DataPreset

        [Header("Data")] 
        public WeaponData data;
        public Transform muzzle;
        public GameObject projectileParent;
        public GameObject casingParent;
        public Transform casingEjectionPoint;
        #endregion

        [Header("Recoil")]
        [SerializeField] private FPSAnimationAsset fireClip;
        [SerializeField] private RecoilAnimData recoilData;
        [SerializeField] private RecoilPatternSettings recoilPatternSettings;
        [SerializeField] private FPSCameraShake cameraShake;
        [Min(0f)] [SerializeField] private float fireRate;

        [SerializeField] private bool supportsAuto;
        [SerializeField] private bool supportsBurst;
        [SerializeField] private int burstLength;

        [Header("Attachments")] 
        
        [SerializeField]
        private AttachmentGroup<BaseAttachment> barrelAttachments = new AttachmentGroup<BaseAttachment>();
        
        [SerializeField]
        private AttachmentGroup<BaseAttachment> gripAttachments = new AttachmentGroup<BaseAttachment>();
        
        [SerializeField]
        private List<AttachmentGroup<ScopeAttachment>> scopeGroups = new List<AttachmentGroup<ScopeAttachment>>();
        
        
        //~ Controller references

        private FPSController _fpsController;
        private Animator _controllerAnimator;
        private UserInputController _userInputController;
        private IPlayablesController _playablesController;
        private FPSCameraController _fpsCameraController;
        
        private FPSAnimator _fpsAnimator;
        private FPSAnimatorEntity _fpsAnimatorEntity;

        private RecoilAnimation _recoilAnimation;
        private RecoilPattern _recoilPattern;
        
        //~ Controller references
        
        private Animator _weaponAnimator;
        private int _scopeIndex;
        
        private float _lastRecoilTime;
        private int _bursts;
        private FireMode _fireMode = FireMode.Semi;
        
        private static readonly int CurveEquip = Animator.StringToHash("CurveEquip");
        private static readonly int CurveUnequip = Animator.StringToHash("CurveUnequip");

        private void Awake()
        {
            SetUp(data.replacement);
            projectileParent = GameObject.Find("Projectiles");
            casingParent = GameObject.Find("Casings");
            
        }


        private void OnActionEnded()
        {
            if (_fpsController == null) return;
            _fpsController.ResetActionState();
        }

        protected void UpdateTargetFOV(bool isAiming)
        {
            float fov = fieldOfView;
            float sensitivityMultiplier = 1f;
            
            if (isAiming && scopeGroups.Count != 0)
            {
                var scope = scopeGroups[_scopeIndex].GetActiveAttachment();
                fov *= scope.aimFovZoom;

                sensitivityMultiplier = scopeGroups[_scopeIndex].GetActiveAttachment().sensitivityMultiplier;
            }

            _userInputController.SetValue("SensitivityMultiplier", sensitivityMultiplier);
            _fpsCameraController.UpdateTargetFOV(fov);
        }

        protected void UpdateAimPoint()
        {
            if (scopeGroups.Count == 0) return;

            var scope = scopeGroups[_scopeIndex].GetActiveAttachment().aimPoint;
            _fpsAnimatorEntity.defaultAimPoint = scope;
        }
        
        protected void InitializeAttachments()
        {
            foreach (var attachmentGroup in scopeGroups)
            {
                attachmentGroup.Initialize(_fpsAnimator);
            }
            
            _scopeIndex = 0;
            if (scopeGroups.Count == 0) return;

            UpdateAimPoint();
            UpdateTargetFOV(false);
        }
        
        public override void OnEquip(GameObject parent)
        {
            if (parent == null) return;
            
            Spawner.Instance.SetData(data, muzzle, this);
            _fpsAnimator = parent.GetComponent<FPSAnimator>();
            _fpsAnimatorEntity = GetComponent<FPSAnimatorEntity>();
            
            _fpsController = parent.GetComponent<FPSController>();
            _weaponAnimator = GetComponentInChildren<Animator>();
            
            _controllerAnimator = parent.GetComponent<Animator>();
            _userInputController = parent.GetComponent<UserInputController>();
            _playablesController = parent.GetComponent<IPlayablesController>();
            _fpsCameraController = parent.GetComponentInChildren<FPSCameraController>();

            if (overrideController != _controllerAnimator.runtimeAnimatorController)
            {
                _playablesController.UpdateAnimatorController(overrideController);
            }
            
            InitializeAttachments();
            
            _recoilAnimation = parent.GetComponent<RecoilAnimation>();
            _recoilPattern = parent.GetComponent<RecoilPattern>();
            
            _fpsAnimator.LinkAnimatorProfile(gameObject);
            
            barrelAttachments.Initialize(_fpsAnimator);
            gripAttachments.Initialize(_fpsAnimator);
            
            _recoilAnimation.Init(recoilData, fireRate, _fireMode);

            if (_recoilPattern != null)
            {
                _recoilPattern.Init(recoilPatternSettings);
            }
            
            _fpsAnimator.LinkAnimatorLayer(equipMotion);
        }

        public override void OnUnEquip()
        {
            _fpsAnimator.LinkAnimatorLayer(unEquipMotion);
        }

        public override bool OnAimPressed()
        {
            _userInputController.SetValue(FPSANames.IsAiming, true);
            UpdateTargetFOV(true);
            _recoilAnimation.isAiming = true;
            
            return true;
        }

        public override bool OnAimReleased()
        {
            _userInputController.SetValue(FPSANames.IsAiming, false);
            UpdateTargetFOV(false);
            _recoilAnimation.isAiming = false;
            
            return true;
        }

        public override bool OnFirePressed()
        {
            // Do not allow firing faster than the allowed fire rate.
            if (Time.unscaledTime - _lastRecoilTime < 60f / fireRate)
            {
                return false;
            }
            
            _lastRecoilTime = Time.unscaledTime;
            _bursts = burstLength;
            
            OnFire();
            
            return true;
        }

        public override bool OnFireReleased()
        {
            if (_recoilAnimation != null)
            {
                _recoilAnimation.Stop();
            }
            
            if (_recoilPattern != null)
            {
                _recoilPattern.OnFireEnd();
            }
            
            CancelInvoke(nameof(OnFire));
            return true;
        }

        public override bool OnReload()
        {
            if (!FPSAnimationAsset.IsValid(reloadClip))
            {
                return false;
            }
            
            _playablesController.PlayAnimation(reloadClip, 0f);
            
            if (_weaponAnimator != null)
            {
                _weaponAnimator.Rebind();
                _weaponAnimator.Play("Reload", 0);
            }

            if (_fpsCameraController != null)
            {
                _fpsCameraController.PlayCameraAnimation(cameraReloadAnimation);
            }
            
            Invoke(nameof(OnActionEnded), reloadClip.clip.length * 0.85f);

            OnFireReleased();
            return true;
        }

        public override bool OnGrenadeThrow()
        {
            if (!FPSAnimationAsset.IsValid(grenadeClip))
            {
                return false;
            }

            _playablesController.PlayAnimation(grenadeClip, 0f);
            
            if (_fpsCameraController != null)
            {
                _fpsCameraController.PlayCameraAnimation(cameraGrenadeAnimation);
            }
            
            Invoke(nameof(OnActionEnded), grenadeClip.clip.length * 0.8f);
            return true;
        }

        /*private FPSProjectiles CreateProjectile(FPSProjectiles projectile, Weapon source, Transform muzzle, Vector3 direction, float speed, float range, Transform parent)
        {
            FPSProjectiles newProjectile = Instantiate(projectile, muzzle.position, muzzle.rotation, parent);
            newProjectile.direction = direction;

            if (_fpsController && _fpsController.characterController)
                newProjectile.shooterVelocity = _fpsController.characterController.velocity;

            newProjectile.speed = speed;
            newProjectile.source = source;
            newProjectile.range = range;
            newProjectile.useAutoScaling = source.data.tracerRounds;
            newProjectile.scaleMultipler = source.data.projectileSize;
            newProjectile.damageRangeCurve = source.data.damageRangeCurve;

            Projectiles?.Add(newProjectile);
            return newProjectile;
        }*/

        public static void UpdateHits(Weapon weapon, FPSProjectiles projectile, GameObject defaultDecal, Ray ray, RaycastHit hit, float damage, float damageRangeFactor, Vector3Direction decalDir)
        {
             //What happens if the bullet hits something  
             if (hit.transform.TryGetComponent(out IgnoreHitDetection ignoreHitDetection)) return;

             FPSHitInfo hitInfo = new FPSHitInfo(projectile, ray, hit);
             GameObject currentDecal = defaultDecal;
             Debug.Log(hit.collider.gameObject.name);

             switch (hit.collider.gameObject.tag)
             {
                 case "Head":
                     weapon._fpsController.PlayerSpirit.TakeDamage(damage, PlayerSpirit.BodyPart.HEAD);
                     break;
                 case "Body":
                     weapon._fpsController.PlayerSpirit.TakeDamage(damage, PlayerSpirit.BodyPart.BODY);
                     break;
                 case "Lower body":
                     weapon._fpsController.PlayerSpirit.TakeDamage(damage, PlayerSpirit.BodyPart.LOWER_BODY);
                     break;
             }

             if (hit.transform.TryGetComponent(out CustomDecal customDecal))
             {
                 currentDecal = customDecal.decalVFX;
             }

             if (currentDecal)
             {
                 Vector3 position = hit.point;
                 Quaternion rotation = MathUtilities.GetFromToRotation(hit, decalDir);

                 GameObject impact = Instantiate(currentDecal, position, rotation);
                 impact.transform.localScale *= weapon.data.decalSize;
                 impact.transform.SetParent(hit.transform);

                 float lifeTime = customDecal ? customDecal.lifeTime : 60;
                 Destroy(impact, lifeTime);
             }

             if (hit.rigidbody)
             {
                 float force = weapon.data.shotDelay <= 0 ? weapon.data.impactForce / weapon.data.shotCount : weapon.data.impactForce;
                 hit.rigidbody.AddForceAtPosition(-hit.normal * force, hit.point, ForceMode.Impulse);
             }
        }

        private void ThrowCasing()
        {
            if (!data.casing || !casingEjectionPoint) return;
            
            Rigidbody newCasing = Instantiate(data.casing, casingEjectionPoint.position, casingEjectionPoint.rotation, casingParent.transform).GetComponent<Rigidbody>();

            Vector3 shooterVelocity = Vector3.zero;
            Vector3 casingVelocity = Vector3.zero;
            
            if (_fpsController && _fpsController.characterController)
                shooterVelocity = _fpsController.characterController.velocity;
            //casingejection = 5
            casingVelocity = transform.GetDirection(data.casingDirection) * data.ejectionVelocity * Random.Range(0.6f, 1);
            casingVelocity += shooterVelocity;
            
            //casingRotation = 25
            newCasing.AddForce(casingVelocity, ForceMode.VelocityChange);
            newCasing.transform.Rotate(Random.Range(-data.casingRotationFactor, data.casingRotationFactor),Random.Range(-data.casingRotationFactor, data.casingRotationFactor), Random.Range(-data.casingRotationFactor, data.casingRotationFactor));
            Destroy(newCasing.gameObject, 5);
            
        }

        private void Fire()
        {
            if (muzzle)
                Debug.Log("Firing from " + muzzle.name);
        }
        
        private void OnFire()
        {
            if (_weaponAnimator != null)
            {
                _weaponAnimator.Play("Fire", 0, 0f);
            }
            
            _fpsCameraController.PlayCameraShake(cameraShake);
            
            if(fireClip != null) _playablesController.PlayAnimation(fireClip);

            if (_recoilAnimation != null && recoilData != null)
            {
                _recoilAnimation.Play();
            }

            if (_recoilPattern != null)
            {
                _recoilPattern.OnFireStart();
            }
            
            //Create projectile
            /*if (data.projectile) {
                FPSProjectiles projectile;
                projectile = CreateProjectile(data.projectile, this, muzzle, muzzle.forward, data.muzzleVelocity, data.range, projectileParent.transform);
                var projectileOnNetwork = projectile.GetComponent<NetworkObject>();
                projectileOnNetwork.Spawn(true);
            }*/
            Spawner.Instance.SpawnBullet();

            ThrowCasing();

            if (_recoilAnimation.fireMode == FireMode.Semi)
            {
                Invoke(nameof(OnFireReleased), 60f / fireRate);
                return;
            }
            
            if (_recoilAnimation.fireMode == FireMode.Burst)
            {
                _bursts--;
                
                if (_bursts == 0)
                {
                    OnFireReleased();
                    return;
                }
            }
            
            Invoke(nameof(OnFire), 60f / fireRate);
        }
        
        public override void OnCycleScope()
        {
            if (scopeGroups.Count == 0) return;
            
            _scopeIndex++;
            _scopeIndex = _scopeIndex > scopeGroups.Count - 1 ? 0 : _scopeIndex;
            
            UpdateAimPoint();
            UpdateTargetFOV(true);
        }

        private void CycleFireMode()
        {
            if (_fireMode == FireMode.Semi && supportsBurst)
            {
                _fireMode = FireMode.Burst;
                _bursts = burstLength;
                return;
            }

            if (_fireMode != FireMode.Auto && supportsAuto)
            {
                _fireMode = FireMode.Auto;
                return;
            }

            _fireMode = FireMode.Semi;
        }
        
        public override void OnChangeFireMode()
        {
            CycleFireMode();
            _recoilAnimation.fireMode = _fireMode;
        }

        public override void OnAttachmentChanged(int attachmentTypeIndex)
        {
            if (attachmentTypeIndex == 1)
            {
                barrelAttachments.CycleAttachments(_fpsAnimator);
                return;
            }

            if (attachmentTypeIndex == 2)
            {
                gripAttachments.CycleAttachments(_fpsAnimator);
                return;
            }

            if (scopeGroups.Count == 0) return;
            scopeGroups[_scopeIndex].CycleAttachments(_fpsAnimator);
            UpdateAimPoint();
        }
    }
}