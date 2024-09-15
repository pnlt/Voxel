using System.Collections;
using System.Collections.Generic;
using Akila.FPSFramework;
using InfimaGames.LowPolyShooterPack._Project.ScriptsPN;
using UnityEditor;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    [CustomEditor(typeof(WeaponData))]
    public class WeaponDataEditor : Editor
    {
        public static bool Foldout_Fire;
        public static bool Foldout_Recoil;
        public static bool Foldout_Movement;
        public static bool Foldout_Audio;

        public override void OnInspectorGUI()
        {
            WeaponData weapon = (WeaponData)target;

            Undo.RecordObject(weapon, $"Modified {weapon}");
            EditorGUI.BeginChangeCheck();

            UpdateBase(weapon);
            EditorGUILayout.Space();

            UpdateFire(weapon);
            //UpdateRecoil(weapon);
            //UpdateMovement(weapon);
            UpdateAudio(weapon);

            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(weapon);
        }

        private void UpdateBase(WeaponData weapon)
        {
            EditorGUILayout.LabelField("Base", EditorStyles.boldLabel);

            weapon.Name = EditorGUILayout.TextField(new GUIContent("Name"), weapon.Name);
            weapon.replacement =
                EditorGUILayout.ObjectField(new GUIContent("Replacement"), weapon.replacement, typeof(FPSPickable),
                    true) as FPSPickable;
            weapon.canves =
                EditorGUILayout.ObjectField(new GUIContent("Canves"), weapon.canves, typeof(WeaponHUD), true) as
                    WeaponHUD;
        }

        private void UpdateFire(WeaponData weapon)
        {
            EditorGUILayout.BeginHorizontal("box");
            weapon.useFire = EditorGUILayout.Toggle(weapon.useFire, GUILayout.MaxWidth(28));
            Foldout_Fire = EditorGUILayout.Foldout(Foldout_Fire, "Fire", true);
            EditorGUILayout.EndHorizontal();

            if (!Foldout_Fire) return;
            EditorGUILayout.BeginVertical("box");

            EditorGUI.BeginDisabledGroup(!weapon.useFire);
            weapon.casingDirection =
                (Vector3Direction)EditorGUILayout.EnumPopup(new GUIContent(" Casing Direction"),
                    weapon.casingDirection);
            
            weapon.shootingMechanism = (Demo.Scripts.Runtime.Item.Weapon.WeaponMethodology)EditorGUILayout.EnumPopup(new GUIContent(" Shooting Mechanism"), weapon.shootingMechanism);
            if(weapon.shootingMechanism == Demo.Scripts.Runtime.Item.Weapon.WeaponMethodology.PROJECTILE)
                weapon.projectile = EditorGUILayout.ObjectField(new GUIContent("FPSProjectile"), weapon.projectile, typeof(FPSProjectiles), false) as FPSProjectiles;
            
            if (!weapon.projectile && weapon.shootingMechanism == Demo.Scripts.Runtime.Item.Weapon.WeaponMethodology.PROJECTILE)
            {
                EditorGUILayout.HelpBox("FPSProjectile must be assgined in order to use this section.", MessageType.Error);
                EditorGUILayout.Space();
            }

            if (weapon.projectile && weapon.shootingMechanism == Demo.Scripts.Runtime.Item.Weapon.WeaponMethodology.PROJECTILE ||
                weapon.shootingMechanism == Demo.Scripts.Runtime.Item.Weapon.WeaponMethodology.HITSCAN)
            {
                weapon.casing =
                    EditorGUILayout.ObjectField(new GUIContent(" Casing"), weapon.casing, typeof(GameObject), false) as
                        GameObject;

                EditorGUILayout.BeginHorizontal();
                weapon.fireRate = EditorGUILayout.FloatField(new GUIContent(" Fire Rate"), weapon.fireRate);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                weapon.muzzleVelocity =
                    EditorGUILayout.FloatField(new GUIContent(" Muzzle Velocity"), weapon.muzzleVelocity);
                EditorGUILayout.EndHorizontal();

                if (weapon.casing != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    weapon.ejectionVelocity = EditorGUILayout.FloatField(new GUIContent(" Casing Ejection Velocity"),
                        weapon.ejectionVelocity);
                    EditorGUILayout.EndHorizontal();
                }

                if (weapon.casing != null)
                {
                    weapon.casingRotationFactor = EditorGUILayout.FloatField(new GUIContent(" Casing Rotation"),
                        weapon.casingRotationFactor);
                }

                weapon.impactForce = EditorGUILayout.FloatField(new GUIContent(" Impact Force"), weapon.impactForce);

                weapon.damage = EditorGUILayout.FloatField(new GUIContent(" Damage"), weapon.damage);

                if (weapon.shootingMechanism == Demo.Scripts.Runtime.Item.Weapon.WeaponMethodology.HITSCAN)
                {
                    weapon.range = EditorGUILayout.FloatField(new GUIContent(" Range"), weapon.range);
                    weapon.defaultDecal =
                        EditorGUILayout.ObjectField(" Default Decal", weapon.defaultDecal, typeof(GameObject), true) as
                            GameObject;
                    weapon.decalDirection =
                        (Vector3Direction)EditorGUILayout.EnumPopup(new GUIContent(" Decal Direction"),
                            weapon.decalDirection);
                }

                weapon.damageRangeCurve = EditorGUILayout.CurveField(" Damage Range Curve", weapon.damageRangeCurve);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(" Gamepad Settings", EditorStyles.boldLabel);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(" Other Settings", EditorStyles.boldLabel);
                weapon.fireTransition =
                    EditorGUILayout.FloatField(new GUIContent(" Fire Transition"), weapon.fireTransition);

                weapon.tracerRounds = EditorGUILayout.Toggle(new GUIContent(" Tracer Rounds"), weapon.tracerRounds);
                weapon.projectileSize =
                    EditorGUILayout.FloatField(new GUIContent(" Projectile Size"), weapon.projectileSize);
                weapon.decalSize = EditorGUILayout.FloatField(new GUIContent(" Decal Size"), weapon.decalSize);
                weapon.afterShotReliefTime = EditorGUILayout.FloatField(new GUIContent(" After Shot Relief Time"),
                    weapon.afterShotReliefTime);
                weapon.shotCount = EditorGUILayout.IntField(new GUIContent(" Shot Count"), weapon.shotCount);

                if (weapon.shotCount > 1)
                {

                    EditorGUILayout.BeginHorizontal();
                    weapon.shotDelay = EditorGUILayout.FloatField(new GUIContent(" Shots Delay"), weapon.shotDelay);
                    EditorGUILayout.EndHorizontal();

                    weapon.alwaysApplyFire =
                        EditorGUILayout.Toggle(new GUIContent(" Always Apply Fire"), weapon.alwaysApplyFire);
                }


                EditorGUILayout.Space();
                EditorGUILayout.LabelField(" Ammunition", EditorStyles.boldLabel);

                weapon.ammoType =
                    EditorGUILayout.ObjectField("Ammo Profile", weapon.ammoType, typeof(AmmoProfileData), true) as
                        AmmoProfileData;

                weapon.magazineCapacity =
                    EditorGUILayout.IntField(new GUIContent(" Magazine Capacity"), weapon.magazineCapacity);
                weapon.reserve = EditorGUILayout.IntField(new GUIContent(" Reserve"), weapon.reserve);

                weapon.automaticReload = EditorGUILayout.Toggle(" Automatic Reload", weapon.automaticReload);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(" Reload", EditorStyles.boldLabel);
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
        }

        /*private void UpdateRecoil(WeaponData weapon)
        {
            EditorGUILayout.BeginHorizontal("box");
            weapon.useRecoil = EditorGUILayout.Toggle(weapon.useRecoil, GUILayout.MaxWidth(28));
            Foldout_Recoil = EditorGUILayout.Foldout(Foldout_Recoil, "Recoil", true);
            EditorGUILayout.EndHorizontal();

            if (!Foldout_Recoil) return;
            EditorGUILayout.BeginVertical("box");

            EditorGUI.BeginDisabledGroup(!weapon.useRecoil);

            EditorGUILayout.LabelField(" Camera", EditorStyles.boldLabel);
            weapon.cameraRecoil = EditorGUILayout.FloatField(" Recoil", weapon.cameraRecoil);
            weapon.cameraShake = EditorGUILayout.FloatField(" Shake", weapon.cameraShake);
            weapon.cameraShakeFadeOutTime = EditorGUILayout.FloatField(" Fadeout Time", weapon.cameraShakeFadeOutTime);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(" Camera Recoil", EditorStyles.boldLabel);
            weapon.horizontalRecoil = EditorGUILayout.FloatField(" Horizontal Recoil", weapon.horizontalRecoil);
            weapon.verticalRecoil = EditorGUILayout.FloatField(" Vertical Recoil", weapon.verticalRecoil);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(" Weapon Recoil", EditorStyles.boldLabel);
            weapon.recoilPositionRoughness = EditorGUILayout.FloatField(" Position Damp Time", weapon.recoilPositionRoughness);
            weapon.recoilRotationRoughness = EditorGUILayout.FloatField(" Rotation Damp Time", weapon.recoilRotationRoughness);

            EditorGUILayout.Space();
            weapon.staticRecoilPosition = EditorGUILayout.Vector3Field(" Static Recoil Position", weapon.staticRecoilPosition);
            weapon.staticRecoilRotation = EditorGUILayout.Vector3Field(" Static Recoil Rotation", weapon.staticRecoilRotation);
            weapon.staticRecoilPositionAim = EditorGUILayout.Vector3Field(" Static Recoil Position Aim", weapon.staticRecoilPositionAim);
            weapon.staticRecoilRotationAim = EditorGUILayout.Vector3Field(" Static Recoil Rotation Aim", weapon.staticRecoilRotationAim);

            EditorGUILayout.Space();
            weapon.recoilPosition = EditorGUILayout.Vector3Field(" Recoil Position", weapon.recoilPosition);
            weapon.recoilRotation = EditorGUILayout.Vector3Field(" Recoil Rotation", weapon.recoilRotation);
            weapon.recoilPositionAim = EditorGUILayout.Vector3Field(" Recoil Position ADS", weapon.recoilPositionAim);
            weapon.recoilRotationAim = EditorGUILayout.Vector3Field(" Recoil Rotation Aim", weapon.recoilRotationAim);

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
        }*/

        /*private void UpdateMovement(WeaponData weapon)
        {
            EditorGUILayout.BeginHorizontal("box");
            weapon.useMovement = EditorGUILayout.Toggle(weapon.useMovement, GUILayout.MaxWidth(28));
            Foldout_Movement = EditorGUILayout.Foldout(Foldout_Movement, "Movement", true);
            EditorGUILayout.EndHorizontal();

            if (!Foldout_Movement) return;
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginDisabledGroup(!weapon.useMovement);

            weapon.Movement_WalkSpeed = EditorGUILayout.FloatField(" Walk Speed", weapon.Movement_WalkSpeed);
            weapon.Movement_RunSpeed = EditorGUILayout.FloatField(" Run Speed", weapon.Movement_RunSpeed);
            weapon.Movement_TacticalSprintSpeed = EditorGUILayout.FloatField(" Tactical Sprint Speed", weapon.Movement_TacticalSprintSpeed);
            weapon.Movement_AimWalkSpeed = EditorGUILayout.FloatField(" Aim Walk Speed", weapon.Movement_AimWalkSpeed);
            weapon.Movement_FireWalkSpeed = EditorGUILayout.FloatField("  Fire Walk Speed", weapon.Movement_FireWalkSpeed);
            weapon.allowFullMovementSpeedWhileAiming = EditorGUILayout.Toggle("Allow Full Movmenet Speed During ADS", weapon.allowFullMovementSpeedWhileAiming);

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
        }*/

        private void UpdateAudio(WeaponData weapon)
        {
            EditorGUILayout.BeginHorizontal("box");
            weapon.useAudio = EditorGUILayout.Toggle(weapon.useAudio, GUILayout.MaxWidth(28));
            Foldout_Audio = EditorGUILayout.Foldout(Foldout_Audio, "Audio", true);
            EditorGUILayout.EndHorizontal();

            if (!Foldout_Audio) return;
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginDisabledGroup(!weapon.useAudio);

            weapon.fire =
                EditorGUILayout.ObjectField(new GUIContent(" Fire"), weapon.fire, typeof(AudioProfile), true) as
                    AudioProfile;

            if (!weapon.fire)
            {
                EditorGUILayout.HelpBox("Fire can't be null.", MessageType.Error);
                EditorGUILayout.Space();
            }

            weapon.fireTail =
                EditorGUILayout.ObjectField(new GUIContent(" Fire Tail"), weapon.fireTail, typeof(AudioProfile), true) as
                    AudioProfile;

            if (!weapon.fireTail)
            {
                EditorGUILayout.HelpBox("Fire trail can't be null.", MessageType.Error);
                EditorGUILayout.Space();
            }

            weapon.reloadAudio =
                EditorGUILayout.ObjectField(new GUIContent(" Reload"), weapon.reloadAudio, typeof(AudioProfile), true) as
                    AudioProfile;

            if (!weapon.reloadAudio)
            {
                EditorGUILayout.HelpBox("Reload can't be null.", MessageType.Error);
                EditorGUILayout.Space();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
        }
    }
}
