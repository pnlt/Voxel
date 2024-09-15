//Copyright 2022, Infima Games. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using InfimaGames.LowPolyShooterPack.Assets_ăn_trộm._External_Assets.Infima_Games.Low_Poly_Shooter_Pack.Code.Client;
using Kart;
using Unity.Netcode;
using UnityEngine;
using Utilities;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Movement. This is our main, and base, component that handles the character's movement.
    /// It contains all of the logic relating to moving, running, crouching, jumping...etc
    /// </summary>

    // Network variables should be value objects
    public struct InputPayload : INetworkSerializable
    {
        public int tick;
        public DateTime timestamp;
        public ulong networkObjectId;
        public Vector3 inputVector;
        public Vector3 position;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref timestamp);
            serializer.SerializeValue(ref networkObjectId);
            serializer.SerializeValue(ref inputVector);
            serializer.SerializeValue(ref position);
        }
    }
    
    public struct StatePayload : INetworkSerializable
    {
        public int tick;
        public ulong networkObjectId;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public Vector3 angularVelocity;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref networkObjectId);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref angularVelocity);
        }
    }
    
    
    public class MovementVjpPro : MovementBehaviour 
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "Acceleration")]
        
        [Tooltip("How fast the character's speed increases.")]
        [SerializeField]
        private float acceleration = 9.0f;

        [Tooltip("Acceleration value used when the character is in the air. This means either jumping, or falling.")]
        [SerializeField]
        private float accelerationInAir = 3.0f;

        [Tooltip("How fast the character's speed decreases.")]
        [SerializeField]
        private float deceleration = 11.0f;
        
        [Title(label: "Speeds")]

        [Tooltip("The speed of the player while walking.")]
        [SerializeField]
        private float speedWalking = 4.0f;
        
        [Tooltip("How fast the player moves while aiming.")]
        [SerializeField]
        private float speedAiming = 3.2f;
        
        [Tooltip("How fast the player moves while aiming.")]
        [SerializeField]
        private float speedCrouching = 3.5f;

        [Tooltip("How fast the player moves while running."), SerializeField]
        private float speedRunning = 6.8f;
        
        [Title(label: "Walking Multipliers")]
        
        [Tooltip("Value to multiply the walking speed by when the character is moving forward."), SerializeField]
        [Range(0.0f, 1.0f)]
        private float walkingMultiplierForward = 1.0f;

        [Tooltip("Value to multiply the walking speed by when the character is moving sideways.")]
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float walkingMultiplierSideways = 1.0f;

        [Tooltip("Value to multiply the walking speed by when the character is moving backwards.")]
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float walkingMultiplierBackwards = 1.0f;
        
        [Title(label: "Air")]

        [Tooltip("How much control the player has over changes in direction while the character is in the air.")]
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float airControl = 0.8f;

        [Tooltip("The value of the character's gravity. Basically, defines how fast the character falls.")]
        [SerializeField]
        private float gravity = 1.1f;

        [Tooltip("The value of the character's gravity while jumping.")]
        [SerializeField]
        private float jumpGravity = 1.0f;

        [Tooltip("The force of the jump.")]
        [SerializeField]
        private float jumpForce = 100.0f;

        [Tooltip("Force applied to keep the character from flying away while descending slopes.")]
        [SerializeField]
        private float stickToGroundForce = 0.03f;

        [Title(label: "Crouching")]

        [Tooltip("Setting this to false will always block the character from crouching.")]
        [SerializeField]
        private bool canCrouch = true;

        [Tooltip("If true, the character will be able to crouch/un-crouch while falling, which can lead to " +
                 "some slightly interesting results.")]
        [SerializeField, ShowIf(nameof(canCrouch), true)]
        private bool canCrouchWhileFalling = false;

        [Tooltip("If true, the character will be able to jump while crouched too!")]
        [SerializeField, ShowIf(nameof(canCrouch), true)]
        private bool canJumpWhileCrouching = true;

        [Tooltip("Height of the character while crouching.")]
        [SerializeField, ShowIf(nameof(canCrouch), true)]
        private float crouchHeight = 1.0f;
        
        [Tooltip("Mask of possible layers that can cause overlaps when trying to un-crouch. Very important!")]
        [SerializeField, ShowIf(nameof(canCrouch), true)]
        private LayerMask crouchOverlapsMask;

        [Title(label: "Rigidbody Push")]

        [Tooltip("Force applied to other rigidbodies when walking into them. This force is multiplied by the character's " +
                 "velocity, so it is never applied by itself, that's important to note.")]
        [SerializeField]
        private float rigidbodyPushForce = 1.0f;
        
        
        #endregion

        #region FIELDS

        /// <summary>
        /// Controller.
        /// </summary>
        private CharacterController controller;

        /// <summary>
        /// Player Character.
        /// </summary>
        private CharacterBehaviour playerCharacter;
        /// <summary>
        /// The player character's equipped weapon.
        /// </summary>
        private WeaponBehaviour equippedWeapon;

        /// <summary>
        /// Default height of the character.
        /// </summary>
        private float standingHeight;

        /// <summary>
        /// Velocity.
        /// </summary>
        private Vector3 velocity;

        /// <summary>
        /// Is the character on the ground.
        /// </summary>
        private bool isGrounded;
        /// <summary>
        /// Was the character standing on the ground last frame.
        /// </summary>
        private bool wasGrounded;

        /// <summary>
        /// Is the character jumping?
        /// </summary>
        private bool jumping;
        /// <summary>
        /// If true, the character controller is crouched.
        /// </summary>
        private bool crouching;

        /// <summary>
        /// Stores the Time.time value when the character last jumped.
        /// </summary>
        private float lastJumpTime;
        
        #endregion

        #region UNITY FUNCTIONS

        /// <summary>
        /// Awake.
        /// </summary>
        
        
        // Netcode general
        NetworkTimer networkTimer;
        const float k_serverTickRate = 60f; // 60 FPS
        const int k_bufferSize = 1024;
        
        // Netcode client specific
        CircularBuffer<StatePayload> clientStateBuffer;
        CircularBuffer<InputPayload> clientInputBuffer;
        StatePayload lastServerState;
        StatePayload lastProcessedState;
        
        ClientNetworkTransform clientNetworkTransform;
        
        // Netcode server specific
        CircularBuffer<StatePayload> serverStateBuffer;
        Queue<InputPayload> serverInputQueue;

        [Header("Netcode")]
        [SerializeField] float reconciliationCooldownTime = 1f;
        [SerializeField] float reconciliationThreshold = 10f;
        [SerializeField] float extrapolationLimit = 0.5f; // 500 miliseconds
        [SerializeField] float extrapolationMultiplier = 1.2f;
        CountdownTimer extrapolationTimer; 
        CountdownTimer reconciliationTimer;
        StatePayload extrapolationState;

        protected override void Awake()
        {
            //Get Player Character.
            playerCharacter = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
            
            //Cache the controller.
            controller = GetComponent<CharacterController>();
            clientNetworkTransform = GetComponent<ClientNetworkTransform>();
            
            //Save the default height.
            standingHeight = controller.height;
            
            networkTimer = new NetworkTimer(k_serverTickRate);
            clientStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
            clientInputBuffer = new CircularBuffer<InputPayload>(k_bufferSize);
            
            serverStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
            serverInputQueue = new Queue<InputPayload>();
            
            reconciliationTimer = new CountdownTimer(reconciliationCooldownTime);
            extrapolationTimer = new CountdownTimer(extrapolationLimit);
            
            reconciliationTimer.OnTimerStart += () =>
            {
                extrapolationTimer.Stop();
            };
            extrapolationTimer.OnTimerStart += () =>
            {
                reconciliationTimer.Stop();
            //    SwitchAuthorityMode(AuthorityMode.Server);
            };
            extrapolationTimer.OnTimerStop += () =>
            {
                extrapolationState = default;
           //     SwitchAuthorityMode(AuthorityMode.Client);
            };
        }

        protected override void Update()
        {
            if (!IsOwner) return;
            
            networkTimer.Update(Time.deltaTime);
            reconciliationTimer.Tick(Time.deltaTime);
            extrapolationTimer.Tick(Time.deltaTime);
            
            // Run on Update or FixedUpdate, or both - depends on the game, consider exposing an option to the editor
            Extrapolate();
        }

        protected override void FixedUpdate()
        {
            while (networkTimer.ShouldTick())
            {
                HandleClientTick();
                HandleServerTick();
            }
            Extrapolate();
        }
        
        void HandleServerTick()
        {
            if (!IsServer) return;
            
            var bufferIndex = -1;

            InputPayload inputPayload = default;
            while (serverInputQueue.Count > 0)
            {
                inputPayload = serverInputQueue.Dequeue();
                
                bufferIndex = inputPayload.tick % k_bufferSize;
                
                StatePayload statePayload = ProcessMovement(inputPayload);
                serverStateBuffer.Add(statePayload, bufferIndex);
            }
            
            if (bufferIndex == -1) return;
            SendToClientRpc(serverStateBuffer.Get(bufferIndex));
            HandleExtrapolation(serverStateBuffer.Get(bufferIndex), CalculateLatencyInMillis(inputPayload));
        }

        static float CalculateLatencyInMillis(InputPayload inputPayload) => (DateTime.Now - inputPayload.timestamp).Milliseconds / 1000f;
        
        void Extrapolate()
        {
           if (IsServer && extrapolationTimer.IsRunning)
           {
               transform.position += extrapolationState.position.With(y: 0);
           }
        }

        void HandleExtrapolation(StatePayload latest, float latency)
        {
            if (ShouldExtrapolate(latency))
            {
                // Calculate the arc the object would traverse in degrees
                float axisLength = latency * latest.angularVelocity.magnitude * Mathf.Rad2Deg;
                Quaternion angularRotation = Quaternion.AngleAxis(axisLength, latest.angularVelocity);
                
                if (extrapolationState.position != default)
                {
                    latest = extrapolationState;
                }
                
                // Update position and rotation based on the extrapolation 
                var posAdjustment = latest.velocity * (1 + latency * extrapolationMultiplier);
                extrapolationState.position = posAdjustment;
                extrapolationState.rotation = angularRotation * transform.rotation;
                extrapolationState.velocity = latest.velocity;
                extrapolationState.angularVelocity = latest.angularVelocity;
                extrapolationTimer.Start();
            }
            else
            {
                extrapolationTimer.Stop();
            }
        }

        private bool ShouldExtrapolate(float latency) => latency < extrapolationLimit && latency > Time.fixedDeltaTime;


        [ClientRpc]
        void SendToClientRpc(StatePayload statePayload)
        {
            if (!IsOwner) return;
            lastServerState = statePayload;
        }

        
        void HandleClientTick()
        {
            if (!IsClient || !IsOwner) return;

            var currentTick = networkTimer.CurrentTick;
            var bufferIndex = currentTick % k_bufferSize;
            
           InputPayload inputPayload = new InputPayload()
            {
                tick = currentTick,
                timestamp = DateTime.Now,
                networkObjectId = NetworkObjectId,
                inputVector = playerCharacter.GetInputMovement(),
                position = transform.position
            };
            
           clientInputBuffer.Add(inputPayload, bufferIndex);
           SendToServerRpc(inputPayload);
            
           StatePayload statePayload = ProcessMovement(inputPayload);
           clientStateBuffer.Add(statePayload, bufferIndex);
           
           HandleServerReconciliation();

        }
        
        bool ShouldReconcile()
        {
            bool isNewServerState = !lastServerState.Equals(default);
            bool isLastStateUndefinedOrDifferent = lastProcessedState.Equals(default) 
                                                   || !lastProcessedState.Equals(lastServerState);
            return isNewServerState && isLastStateUndefinedOrDifferent && !reconciliationTimer.IsRunning && !extrapolationTimer.IsRunning;
        }

        void HandleServerReconciliation()
        {
            if (!ShouldReconcile()) return;

            float positionError;
            int bufferIndex;
            
            bufferIndex = lastServerState.tick % k_bufferSize;
            if (bufferIndex - 1 < 0) return; // Not enough information to reconcile
            
            StatePayload rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState; // Host RPCs execute immediately, so we can use the last server state
            StatePayload clientState = IsHost ? clientStateBuffer.Get(bufferIndex - 1) : clientStateBuffer.Get(bufferIndex);
            positionError = Vector3.Distance(rewindState.position, clientState.position);

            if (positionError > reconciliationThreshold)
            {
               ReconcileState(rewindState); 
               reconciliationTimer.Start();
            }

            lastProcessedState = rewindState;
        }
        
        void ReconcileState(StatePayload rewindState)
        {
            transform.position = rewindState.position;
            //transform.rotation = rewindState.rotation;
            velocity = rewindState.velocity;

            if (!rewindState.Equals(lastServerState)) return;
            
            clientStateBuffer.Add(rewindState, rewindState.tick % k_bufferSize);
            
            // Replay all inputs front the rewind state to the current state
            int tickToReplay = lastServerState.tick;
            
            while (tickToReplay < networkTimer.CurrentTick)
            {
                int bufferIndex = tickToReplay % k_bufferSize;
                StatePayload statePayload = ProcessMovement(clientInputBuffer.Get(bufferIndex));
                clientStateBuffer.Add(statePayload, bufferIndex);
                tickToReplay++;
            }
        }
        
        [ServerRpc]
        void SendToServerRpc(InputPayload input)
        {
            serverInputQueue.Enqueue(input);
        }
        
        StatePayload ProcessMovement(InputPayload input)
        {
            Move();
            
            return new StatePayload()
            {
                tick = input.tick,
                networkObjectId = NetworkObjectId,
                position = transform.position,
                rotation = transform.rotation,
                velocity = velocity,
                angularVelocity = Vector3.zero
            };
        }

        void Move()
        {

            //Get the equipped weapon!
            equippedWeapon = playerCharacter.GetInventory().GetEquipped();

            //Get this frame's grounded value.
            isGrounded = IsGrounded();
            //Check if it has changed from last frame.
            if (isGrounded && !wasGrounded)
            {
                //Set jumping.
                jumping = false;
                //Set lastJumpTime.
                lastJumpTime = 0.0f;
            }
            else if (wasGrounded && !isGrounded)
                lastJumpTime = Time.time;
            
            //Move.
            MoveCharacter();
            //Save the grounded value to check for difference next frame.
            wasGrounded = isGrounded;
            
        }
        /// <summary>
        /// OnControllerColliderHit.
        /// </summary>
        protected void OnControllerColliderHit(ControllerColliderHit hit)
        {
            //Zero out the upward velocity if the character hits the ceiling.
            if (hit.moveDirection.y > 0.0f && velocity.y > 0.0f)
                velocity.y = 0.0f;

            //We need a rigidbody to push the object we just hit.
            Rigidbody hitRigidbody = hit.rigidbody;
            if (hitRigidbody == null)
                return;
            
            //AddForce.
            Vector3 force = (hit.moveDirection + Vector3.up * 0.35f) * velocity.magnitude * rigidbodyPushForce;
            hitRigidbody.AddForceAtPosition(force, hit.point);
        }
        
        #endregion

        #region METHODS


        /// <summary>
        /// Moves the character.
        /// </summary>
        private void MoveCharacter()
        {
            if (!IsOwner) return;
            //Get Movement Input!
            Vector2 frameInput = Vector3.ClampMagnitude(playerCharacter.GetInputMovement(), 1.0f);
            //Calculate local-space direction by using the player's input.
            var desiredDirection = new Vector3(frameInput.x, 0.0f, frameInput.y);
            
            //Running speed calculation.
            if(playerCharacter.IsRunning())
                desiredDirection *= speedRunning;
            else
            {
                //Crouching Speed.
                if (crouching)
                    desiredDirection *= speedCrouching;
                else
                {
                    //Aiming speed calculation.
                    if (playerCharacter.IsAiming())
                        desiredDirection *= speedAiming;
                    else
                    {
                        //Multiply by the normal walking speed.
                        desiredDirection *= speedWalking;
                        //Multiply by the sideways multiplier, to get better feeling sideways movement.
                        desiredDirection.x *= walkingMultiplierSideways;
                        //Multiply by the forwards and backwards multiplier.
                        desiredDirection.z *=
                            (frameInput.y > 0 ? walkingMultiplierForward : walkingMultiplierBackwards);
                    }
                }
            } 

            //World space velocity calculation.
            desiredDirection = transform.TransformDirection(desiredDirection);
            //Multiply by the weapon movement speed multiplier. This helps us modify speeds based on the weapon!
            if (equippedWeapon != null)
                desiredDirection *= equippedWeapon.GetMultiplierMovementSpeed();
            
            //Apply gravity!
            if (isGrounded == false)
            {
                //Get rid of any upward velocity.
                if (wasGrounded && !jumping)
                    velocity.y = 0.0f;
                
                //Movement.
                velocity += desiredDirection * (accelerationInAir * airControl * Time.deltaTime);
                //Gravity.
                velocity.y -= (velocity.y >= 0 ? jumpGravity : gravity) * Time.deltaTime;
            }
            //Normal Movement On Ground.
            else if(!jumping)
            {
                //Update velocity with movement on the ground values.
                velocity = Vector3.Lerp(velocity, new Vector3(desiredDirection.x, velocity.y, desiredDirection.z), networkTimer.MinTimeBetweenTicks * (desiredDirection.sqrMagnitude > 0.0f ? acceleration : deceleration));
            }

            //Velocity Applied.
            Vector3 applied = velocity * Time.deltaTime;
            //Stick To Ground Force. Helps with making the character walk down slopes without floating.
            if (controller.isGrounded && !jumping)
                applied.y = -stickToGroundForce;

            //Move.
            controller.Move(applied);
        }

        /// <summary>
        /// WasGrounded.
        /// </summary>
        public override bool WasGrounded() => wasGrounded;
        /// <summary>
        /// IsJumping.
        /// </summary>
        public override bool IsJumping() => jumping;

        /// <summary>
        /// Can Crouch.
        /// </summary>
        public override bool CanCrouch(bool newCrouching)
        {
            //Always block crouching if we need to.
            if (canCrouch == false)
                return false;

            //If we're in the air, and we cannot crouch while in the air, then we can ignore this execution!
            if (isGrounded == false && canCrouchWhileFalling == false)
                return false;
            
            //The controller can always crouch, the issue is un-crouching!
            if (newCrouching)
                return true;

            //Overlap check location.
            Vector3 sphereLocation = transform.position + Vector3.up * standingHeight;
            //Check for any overlaps.
            return (Physics.OverlapSphere(sphereLocation, controller.radius, crouchOverlapsMask).Length == 0);
        }

        /// <summary>
        /// IsCrouching.
        /// </summary>
        /// <returns></returns>
        public override bool IsCrouching() => crouching;

        /// <summary>
        /// Jump.
        /// </summary>
        public override void Jump()
        {
            //We can ignore this if we're crouching and we're not allowed to do crouch-jumps.
            if (crouching && !canJumpWhileCrouching)
                return;
            
            //Block jumping when we're not grounded. This avoids us double jumping.
            if (!isGrounded)
                return;

            //Jump.
            jumping = true;
            //Apply Jump Velocity.
            velocity = new Vector3(velocity.x, Mathf.Sqrt(2.0f * jumpForce * jumpGravity), velocity.z);

            //Save lastJumpTime.
            lastJumpTime = Time.time;
        }
        /// <summary>
        /// Changes the controller's capsule height.
        /// </summary>
        public override void Crouch(bool newCrouching)
        {
            //Set the new crouching value.
            crouching = newCrouching;
            
            //Update the capsule's height.
            controller.height = crouching ? crouchHeight : standingHeight;
            //Update the capsule's center.
            controller.center = controller.height / 2.0f * Vector3.up;
        }

        public override void TryCrouch(bool value)
        {
            //Crouch.
            if (value && CanCrouch(true))
                Crouch(true);
            //Coroutine Un-Crouch.
            else if(!value)
                StartCoroutine(nameof(TryUncrouch));
        }

        /// <summary>
        /// Try Toggle Crouch.
        /// </summary>
        public override void TryToggleCrouch() => TryCrouch(!crouching);
        /// <summary>
        /// Tries to un-crouch the character.
        /// </summary>
        private IEnumerator TryUncrouch()
        {
            //If the movementBehaviour says that we can't go into whatever crouching state is the opposite, then
            //the character will have to forget about it, no way around it bois!
            yield return new WaitUntil(() => CanCrouch(false));
            
            //Un-Crouch.
            Crouch(false);
        }

        #endregion

        #region GETTERS

        /// <summary>
        /// GetLastJumpTime.
        /// </summary>
        public override float GetLastJumpTime() => lastJumpTime;

        /// <summary>
        /// Get Multiplier Forward.
        /// </summary>
        public override float GetMultiplierForward() => walkingMultiplierForward;
        /// <summary>
        /// Get Multiplier Sideways.
        /// </summary>
        public override float GetMultiplierSideways() => walkingMultiplierSideways;
        /// <summary>
        /// Get Multiplier Backwards.
        /// </summary>
        public override float GetMultiplierBackwards() => walkingMultiplierBackwards;
        
        /// <summary>
        /// Returns the value of Velocity.
        /// </summary>
        public override Vector3 GetVelocity() => controller.velocity;
        /// <summary>
        /// Returns the value of Grounded.
        /// </summary>
        public override bool IsGrounded() => controller.isGrounded;

        #endregion
    }
}