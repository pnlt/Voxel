using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.FPSAnimationFramework.Runtime.Recoil;
using KINEMATION.KAnimationCore.Runtime.Input;
using KINEMATION.KAnimationCore.Runtime.Rig;

using Demo.Scripts.Runtime.Item;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Demo.Scripts.Runtime.Character
{
    public enum FPSAimState
    {
        None,
        Ready,
        Aiming,
        PointAiming
    }

    public enum FPSActionState
    {
        None,
        PlayingAnimation,
        WeaponChange,
        AttachmentEditing
    }

    [RequireComponent(typeof(CharacterController), typeof(FPSMovement))]
    public class FPSController : NetworkBehaviour
    {
        public bool isHit = false;
        //~ Legacy Controller Interface
        [SerializeField] private FPSControllerSettings settings;
        [SerializeField] private int maxSlot;
        [Header("Refs")] 
        [SerializeField] private Camera mainCam;
        [SerializeField] private float positionRange = 5f;
        //[SerializeField] AudioListener playerAudioListener;
        public List<Weapon> weapons = new List<Weapon>();
        
        // NetworkVariables for position and rotation
        private NetworkVariable<Vector3> netPosition = new NetworkVariable<Vector3>();
        private NetworkVariable<Quaternion> netRotation = new NetworkVariable<Quaternion>();
        private NetworkVariable<Quaternion> netCameraRotation = new NetworkVariable<Quaternion>();

        // Local variables to store smooth values
        private Vector3 smoothPosition;
        private Quaternion smoothRotation;
        private Quaternion smoothCameraRotation;

        // Smoothing factor
        public float smoothFactor = 15f;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                mainCam.enabled = false;
            }
            else
            {
                mainCam.enabled = true;
				UpdatePositionServerRpc();
            }
        }
        [ServerRpc(RequireOwnership = false)]
        private void UpdatePositionServerRpc()
        {
           transform.position = new Vector3(Random.Range(positionRange, -positionRange), 0, Random.Range(positionRange, -positionRange));
        }

        public PlayerSpirit PlayerSpirit
        {
            get
            {
                return GetComponent<PlayerSpirit>();
            }
        }
        

        public int MaxSlot
        {
            get
            {
                return this.maxSlot;
            }
        }
        public CharacterController characterController { get; private set; }

        private FPSMovement _movementComponent;

        private Transform _weaponBone;
        private Vector2 _playerInput;

        private int _activeWeaponIndex;
        private int _previousWeaponIndex;

        private FPSAimState _aimState;
        private FPSActionState _actionState;

        private Animator _animator;
        
        //~ Legacy Controller Interface

        // ~Scriptable Animation System Integration
        private FPSAnimator _fpsAnimator;
        private UserInputController _userInput;
        // ~Scriptable Animation System Integration

        private List<FPSItem> _instantiatedWeapons;

        public List<FPSItem> Items
        {
            get { return _instantiatedWeapons; }
        }
        private Vector2 _lookDeltaInput;

        private RecoilPattern _recoilPattern;
        private int _sensitivityMultiplierPropertyIndex;

        private static int _fullBodyWeightHash = Animator.StringToHash("FullBodyWeight");
        private static int _proneWeightHash = Animator.StringToHash("ProneWeight");
        private static int _inspectStartHash = Animator.StringToHash("InspectStart");
        private static int _inspectEndHash = Animator.StringToHash("InspectEnd");
        private static int _slideHash = Animator.StringToHash("Sliding");

        private void PlayTransitionMotion(FPSAnimatorLayerSettings layerSettings)
        {
            if (layerSettings == null)
            {
                return;
            }
            
            _fpsAnimator.LinkAnimatorLayer(layerSettings);
        }

        private bool IsSprinting()
        {
            return _movementComponent.MovementState == FPSMovementState.Sprinting;
        }
        
        private bool HasActiveAction()
        {
            return _actionState != FPSActionState.None;
        }

        private bool IsAiming()
        {
            return _aimState is FPSAimState.Aiming or FPSAimState.PointAiming;
        }

        private void InitializeMovement()
        {
            _movementComponent = GetComponent<FPSMovement>();
            
            _movementComponent.onJump = () => { PlayTransitionMotion(settings.jumpingMotion); };
            _movementComponent.onLanded = () => { PlayTransitionMotion(settings.jumpingMotion); };

            _movementComponent.onCrouch = OnCrouch;
            _movementComponent.onUncrouch = OnUncrouch;

            _movementComponent.onSprintStarted = OnSprintStarted;
            _movementComponent.onSprintEnded = OnSprintEnded;

            _movementComponent.onSlideStarted = OnSlideStarted;

            _movementComponent._slideActionCondition += () => !HasActiveAction();
            _movementComponent._sprintActionCondition += () => !HasActiveAction();
            _movementComponent._proneActionCondition += () => !HasActiveAction();
            
            _movementComponent.onStopMoving = () =>
            {
                PlayTransitionMotion(settings.stopMotion);
            };
            
            _movementComponent.onProneEnded = () =>
            {
                _userInput.SetValue(FPSANames.PlayablesWeight, 1f);
            };
        }

        private void InitializeWeapons()
        {
            _instantiatedWeapons = new List<FPSItem>();

            foreach (var prefab in settings.weaponPrefabs)
            {
                AddItem(prefab);
            }
        }

        private void Start()
        {
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            _weaponBone = GetComponentInChildren<KRigComponent>().GetRigTransform(settings.weaponBone);
            _fpsAnimator = GetComponent<FPSAnimator>();
            _animator = GetComponent<Animator>();
            
            _userInput = GetComponent<UserInputController>();
            _recoilPattern = GetComponent<RecoilPattern>();

            InitializeMovement();
            InitializeWeapons();

            _actionState = FPSActionState.None;
            EquipWeapon();

            _sensitivityMultiplierPropertyIndex = _userInput.GetPropertyIndex("SensitivityMultiplier");
        }

        public void ResetActionState()
        {
            _actionState = FPSActionState.None;
        }

        private void DisableAim()
        {
            if (GetActiveItem().OnAimReleased()) _aimState = FPSAimState.None;
        }
        
        [ServerRpc]
        private void FireWeaponServerRpc()
        {
            // Fire the active weapon on the server side
            GetActiveItem()?.OnFirePressed();
            FireWeaponClientRpc();
        }
        [ClientRpc]
        private void FireWeaponClientRpc()
        {
            GetActiveItem()?.OnFirePressed(); // Trigger fire animation on client
        }

        [ServerRpc]
        private void StopFiringWeaponServerRpc()
        {
            // Stop firing the active weapon on the server side
            GetActiveItem()?.OnFireReleased();
            StopFiringWeaponClientRpc();
        }
        [ClientRpc]
        private void StopFiringWeaponClientRpc()
        {
            GetActiveItem()?.OnFireReleased();
        }
        
        private void OnFirePressed()
        {
            if (!IsOwner || _instantiatedWeapons.Count == 0 || HasActiveAction()) return;
            // Send fire action to the server
            FireWeaponServerRpc();
        }

        private void OnFireReleased()
        {
           if (!IsOwner || _instantiatedWeapons.Count == 0) return;
            // Send stop fire action to the server
            StopFiringWeaponServerRpc();
        }

        public FPSItem GetActiveItem()
        {
            if (_instantiatedWeapons.Count == 0) return null;
            return _instantiatedWeapons[_activeWeaponIndex];
        }
        
        private void OnSlideStarted()
        {
            _animator.CrossFade(_slideHash, 0.2f);
        }
        
        private void OnSprintStarted()
        {
            OnFireReleased();
            DisableAim();

            _aimState = FPSAimState.None;

            _userInput.SetValue(FPSANames.StabilizationWeight, 0f);
            _userInput.SetValue("LookLayerWeight", 0.3f);
        }

        private void OnSprintEnded()
        {
            _userInput.SetValue(FPSANames.StabilizationWeight, 1f);
            _userInput.SetValue("LookLayerWeight", 1f);
        }

        private void OnCrouch()
        {
            PlayTransitionMotion(settings.crouchingMotion);
        }

        private void OnUncrouch()
        {
            PlayTransitionMotion(settings.crouchingMotion);
        }
        
        private bool _isLeaning;

        [ServerRpc]
        private void UpdatePlayerLookServerRpc(Vector3 position, float yaw)
        {
            transform.position = position;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
        
      //  private void UpdateLookInput()
      //  {
      //      if (!IsOwner) return;
      //      
      //      float scale = _userInput.GetValue<float>(_sensitivityMultiplierPropertyIndex);
      //      
      //      float deltaMouseX = _lookDeltaInput.x * settings.sensitivity * scale;
      //      float deltaMouseY = -_lookDeltaInput.y * settings.sensitivity * scale;
      //      
      //      _playerInput.y += deltaMouseY;
      //      _playerInput.x += deltaMouseX;
      //      
      //      if (_recoilPattern != null)
      //      {
      //          _playerInput += _recoilPattern.GetRecoilDelta();
      //          deltaMouseX += _recoilPattern.GetRecoilDelta().x;
      //      }
      //      
      //      float proneWeight = _animator.GetFloat(_proneWeightHash);
      //      Vector2 pitchClamp = Vector2.Lerp(new Vector2(-90f, 90f), new Vector2(-30, 0f), proneWeight);

      //      _playerInput.y = Mathf.Clamp(_playerInput.y, pitchClamp.x, pitchClamp.y);
      //      
      //      transform.rotation *= Quaternion.Euler(0f, deltaMouseX, 0f);
      //      
      //      // Update camera rotation (pitch) for up/down movement
      //      mainCam.transform.localRotation = Quaternion.Euler(_playerInput.y, 0f, 0f);
      //      
      //      _userInput.SetValue(FPSANames.MouseDeltaInput, new Vector4(deltaMouseX, deltaMouseY));
      //      _userInput.SetValue(FPSANames.MouseInput, new Vector4(_playerInput.x, _playerInput.y));
      //      
      //      // Send updated yaw (body rotation) to the server
      //      UpdatePlayerLookServerRpc(transform.position, transform.rotation.eulerAngles.y);

      //      // Send updated pitch (camera rotation) to the server
      //      UpdatePlayerPitchServerRpc(_playerInput.y);
      //  }
      
      private void UpdateLookInput()
      {
          if (!IsOwner) return;
            
          float scale = _userInput.GetValue<float>(_sensitivityMultiplierPropertyIndex);
            
          float deltaMouseX = _lookDeltaInput.x * settings.sensitivity * scale;
          float deltaMouseY = -_lookDeltaInput.y * settings.sensitivity * scale;
            
          _playerInput.y += deltaMouseY;
          _playerInput.x += deltaMouseX;
            
          if (_recoilPattern != null)
          {
              _playerInput += _recoilPattern.GetRecoilDelta();
              deltaMouseX += _recoilPattern.GetRecoilDelta().x;
          }
            
          float proneWeight = _animator.GetFloat(_proneWeightHash);
          Vector2 pitchClamp = Vector2.Lerp(new Vector2(-90f, 90f), new Vector2(-30, 0f), proneWeight);

          _playerInput.y = Mathf.Clamp(_playerInput.y, pitchClamp.x, pitchClamp.y);
            
          transform.rotation *= Quaternion.Euler(0f, deltaMouseX, 0f);
          mainCam.transform.localRotation = Quaternion.Euler(_playerInput.y, 0f, 0f);
            
          _userInput.SetValue(FPSANames.MouseDeltaInput, new Vector4(deltaMouseX, deltaMouseY));
          _userInput.SetValue(FPSANames.MouseInput, new Vector4(_playerInput.x, _playerInput.y));
      }
        
        [ServerRpc]
        private void UpdatePlayerPitchServerRpc(float pitch)
        {
            // Broadcast pitch to all clients
            UpdatePlayerPitchClientRpc(pitch);
        }

        [ClientRpc]
        private void UpdatePlayerPitchClientRpc(float pitch)
        {
            if (IsOwner) return;  // Skip if this is the owner

            // Apply pitch to the camera's local rotation on all clients
            mainCam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }


        private void OnMovementUpdated()
        {
            float playablesWeight = 1f - _animator.GetFloat(_fullBodyWeightHash);
            _userInput.SetValue(FPSANames.PlayablesWeight, playablesWeight);
        }

       private void Update()
       {
           if (IsOwner)
           {
               UpdateOwnerMovement();
           }
           else
           {
               UpdateClientMovement();
           }
           Time.timeScale = settings.timeScale;
           OnMovementUpdated();
       }
       private void UpdateOwnerMovement()
       {
           // Instead of directly applying rotations, update the NetworkVariables
           UpdateLookInput();
           
           // Send updated values to the server
           UpdateTransformServerRpc(transform.position, transform.rotation, mainCam.transform.localRotation);

       }
       
       [ServerRpc]
       private void UpdateTransformServerRpc(Vector3 position, Quaternion rotation, Quaternion cameraRotation)
       {
           // Update NetworkVariables on the server
           netPosition.Value = position;
           netRotation.Value = rotation;
           netCameraRotation.Value = cameraRotation;
       }
       private void UpdateClientMovement()
       {
           // Smoothly interpolate position and rotation for non-owner clients
           smoothPosition = Vector3.Lerp(smoothPosition, netPosition.Value, Time.deltaTime * smoothFactor);
           smoothRotation = Quaternion.Slerp(smoothRotation, netRotation.Value, Time.deltaTime * smoothFactor);
           smoothCameraRotation = Quaternion.Slerp(smoothCameraRotation, netCameraRotation.Value, Time.deltaTime * smoothFactor);

           // Apply smoothed values
           transform.position = smoothPosition;
           transform.rotation = smoothRotation;
           mainCam.transform.localRotation = smoothCameraRotation;
       }

        #region Equipment Handler

        [SerializeField] private float force = 1;
        [SerializeField] private Transform dropLocation;

        public void RemoveItem(FPSItem item)
        {
            _instantiatedWeapons.Remove(item);
            _activeWeaponIndex -= 1;
            if (_previousWeaponIndex != 0) _previousWeaponIndex = _activeWeaponIndex - 1;
        }

        public void AddItem(FPSItem item)
        {

            var weapon = Instantiate(item, transform.position, Quaternion.identity);

            var weaponTransform = weapon.transform;

            weaponTransform.parent = _weaponBone;
            weaponTransform.localPosition = Vector3.zero;
            weaponTransform.localRotation = Quaternion.identity;

            _instantiatedWeapons.Add(weapon);
            weapon.gameObject.SetActive(false);
        }
        
        public void ReplaceItem(FPSItem oldItem, FPSItem newItem)
        {
            AddItem(newItem);
            oldItem.Drop(transform.forward * force, transform.right * force, dropLocation, this);
        }

        //Conflict
        public void ChangeWeapon(FPSItem.WeaponState state)
        {
            if (_movementComponent.PoseState == FPSPoseState.Prone) return;
            if (!IsOwner || HasActiveAction() || _instantiatedWeapons.Count == 0) return;

            if (_activeWeaponIndex == _instantiatedWeapons.Count - 1 && state == FPSItem.WeaponState.DROP)
            {
                ChangeWeaponServerRpc(_activeWeaponIndex - 1, state);
            }
            // Send the new weapon index to the server
            else
            {
                ChangeWeaponServerRpc(_activeWeaponIndex + 1 > _instantiatedWeapons.Count - 1 ? 0 : _activeWeaponIndex + 1, state);
            }
            
        }

        private void UnequipWeapon()
        {
            DisableAim();
            _actionState = FPSActionState.WeaponChange;
            GetActiveItem().OnUnEquip();
        }

        [ClientRpc]
        private void EquipWeaponClientRpc(int newWeaponIndex, FPSItem.WeaponState state)
        {
            StartWeaponChange(newWeaponIndex, state);
        }
        
        [ServerRpc]
        private void EquipWeaponServerRpc(int newWeaponIndex, FPSItem.WeaponState state)
        {
            // Execute the weapon change on the server
           EquipWeaponClientRpc(newWeaponIndex, state);
        }
        
        private void EquipWeapon()
        {
            if (_instantiatedWeapons.Count == 0) return;

            _instantiatedWeapons[_previousWeaponIndex].gameObject.SetActive(false);
            GetActiveItem().gameObject.SetActive(true);
            GetActiveItem().OnEquip(gameObject);

            _actionState = FPSActionState.None;
        }
        
        //Conflict
        private void StartWeaponChange(int newIndex, FPSItem.WeaponState state)
        {
            if (newIndex == _activeWeaponIndex || newIndex > _instantiatedWeapons.Count - 1)
            {
                return;
            }

            UnequipWeapon();

            OnFireReleased();
            Invoke(nameof(EquipWeapon), settings.equipDelay);

            if (_activeWeaponIndex < _instantiatedWeapons.Count - 1 || state == FPSItem.WeaponState.NONE)
            {
                _previousWeaponIndex = _activeWeaponIndex;
                _activeWeaponIndex = newIndex;
            }
        }
        #endregion

#if ENABLE_INPUT_SYSTEM
        [ServerRpc(RequireOwnership = false)]
        private void DropWeaponServerRpc()
        {
            DropWeaponClientRpc();
        }

        [ClientRpc]
        private void DropWeaponClientRpc()
        {
            _instantiatedWeapons[_activeWeaponIndex]?.Drop(Vector3.down * force, Vector3.up * force, dropLocation, this);
        }
        
        public void OnDrop()
        {
            if (!IsOwner) return;

            // Send weapon drop action to the server
            //DropWeaponServerRpc();
        }
        
        [ServerRpc]
        private void ReloadAmmoServerRpc()
        {
            if (IsSprinting() || HasActiveAction() || !GetActiveItem().OnReload()) return;
            _actionState = FPSActionState.PlayingAnimation;
            
            // Call the ClientRpc to synchronize reload across all clients
            ReloadAmmoClientRpc();
        }
        
        [ClientRpc]
        private void ReloadAmmoClientRpc()
        {
            // Perform reload on the client side
            if (IsSprinting() || HasActiveAction() || !GetActiveItem().OnReload()) return;
            _actionState = FPSActionState.PlayingAnimation;
        }
        
        public void OnReload()
        {
            if (!IsOwner) return;
            ReloadAmmoServerRpc();
        }
        
        [ServerRpc]
        private void ThrowGrenadeServerRpc()
        {
            if (IsSprinting() || HasActiveAction() || !GetActiveItem().OnGrenadeThrow()) return;
            _actionState = FPSActionState.PlayingAnimation;
        }

        public void OnThrowGrenade()
        {
            if (!IsOwner) return;

            // Send grenade throw action to the server
            ThrowGrenadeServerRpc();
        }
        
        public void OnFire(InputValue value)
        {
           // if (!IsOwner) return;
            
            if (IsSprinting()) return;
            
            if (value.isPressed)
            {
                OnFirePressed();
                return;
            }
            
            OnFireReleased();
        }
        
        [ServerRpc]
        private void StartAimingServerRpc()
        {
            StartAimingClientRpc();
        }

        [ClientRpc]
        private void StartAimingClientRpc()
        {
            if (_instantiatedWeapons.Count == 0) return;

            // Execute aim logic on the server
            if (GetActiveItem().OnAimPressed()) 
            {
                _aimState = FPSAimState.Aiming;
                PlayTransitionMotion(settings.aimingMotion);
            }
        }

        [ServerRpc]
        private void StopAimingServerRpc()
        {
            StopAimingClientRpc();
        }

        [ClientRpc]
        private void StopAimingClientRpc()
        {
            if (_instantiatedWeapons.Count == 0) return;

            // Execute aim stop logic on the server
            DisableAim();
            PlayTransitionMotion(settings.aimingMotion);
        }

        public void OnAim(InputValue value)
        {
            if (!IsOwner || IsSprinting()) return;

            // Start Aiming
            if (value.isPressed && !IsAiming())
            {
                // Send aim action to the server
                StartAimingServerRpc();
                return;
            }

            // Stop Aiming
            if (!value.isPressed && IsAiming())
            {
                // Send stop aim action to the server
                StopAimingServerRpc();
            }
        }

        
        [ServerRpc]
        private void ChangeWeaponServerRpc(int newWeaponIndex, FPSItem.WeaponState state)
        {
            // Call ClientRpc to synchronize the weapon equip across all clients
            EquipWeaponClientRpc(newWeaponIndex, state);
        }

        public void OnChangeWeapon()
        {
            ChangeWeapon(FPSItem.WeaponState.NONE);
        }

        public void OnLook(InputValue value)
        {
            _lookDeltaInput = value.Get<Vector2>();
        }

        public void OnLean(InputValue value)
        {
            _userInput.SetValue(FPSANames.LeanInput, value.Get<float>() * settings.leanAngle);
            PlayTransitionMotion(settings.leanMotion);
        }

        public void OnCycleScope()
        {
            if (!IsAiming()) return;
            
            GetActiveItem().OnCycleScope();
            PlayTransitionMotion(settings.aimingMotion);
        }

        public void OnChangeFireMode()
        {
            GetActiveItem().OnChangeFireMode();
        }

        public void OnToggleAttachmentEditing()
        {
            if (HasActiveAction() && _actionState != FPSActionState.AttachmentEditing) return;
            
            _actionState = _actionState == FPSActionState.AttachmentEditing 
                ? FPSActionState.None : FPSActionState.AttachmentEditing;

            if (_actionState == FPSActionState.AttachmentEditing)
            {
                _animator.CrossFade(_inspectStartHash, 0.2f);
                return;
            }
            
            _animator.CrossFade(_inspectEndHash, 0.3f);
        }

        public void OnDigitAxis(InputValue value)
        {
            if (!value.isPressed || _actionState != FPSActionState.AttachmentEditing) return;
            GetActiveItem().OnAttachmentChanged((int) value.Get<float>());
        }
#endif
    }
}