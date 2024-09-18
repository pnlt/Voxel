using System.Collections;
using Akila.FPSFramework;
using Demo.Scripts.Runtime.Character;
using Unity.Netcode;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack._Project.ScriptsPN
{
    //[AddComponentMenu("Akila/FPS Framework/Weapons/Projectile")]
    public class FPSProjectiles : NetworkBehaviour
    {
        [Header("Base Settings")]
        public LayerMask hittableLayers = -1;
        public Vector3Direction decalDirection = Vector3Direction.forward;
        public float penetrationStrenght = 100;
        public float speed = 50;
        public float gravity = 1;
        public float force = 10;
        public int lifeTime = 5;
        public GameObject defaultDecal;
        public Spawner parent;

        [Header("Additional Settings")]
        public bool destroyOnImpact = false;
        public bool useSourceVelocity = true;
        public bool useAutoScaling = true;
        public float scaleMultipler = 45;

        [Header("Range Control")]
        public float range = 300;
        public AnimationCurve damageRangeCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 0.3f) });


        public Demo.Scripts.Runtime.Item.Weapon source { get; set; }
        public Vector3 direction { get; set; }
        public Vector3 shooterVelocity { get; set; }
        private int damage;
        private float damageRangeFactor;
        private float maxVelocity;
        private Vector3 velocity;
        private TrailRenderer trail;
        private Explosive explosive;

        private Rigidbody rb;

        private Vector3 previousPosition;

        private Transform Effects;

        private Vector3 startPosition;
        
        private bool isInitialized = false;
        private bool isSourceSet = false;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                //Initialize();
                StartCoroutine(WaitForSourceAndInitialize());
            }
            else
            {
                //InitializeServerRpc();
                WaitForSourceAndInitializeServerRpc();
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void WaitForSourceAndInitializeServerRpc()
        {
            StartCoroutine(WaitForSourceAndInitialize());
        }

        private IEnumerator WaitForSourceAndInitialize()
        {
            float timeout = 5f; // 5 seconds timeout
            float elapsedTime = 0f;

            while (!isSourceSet && elapsedTime < timeout)
            {
                if (source != null && source.data != null)
                {
                    isSourceSet = true;
                    Initialize();
                    break;
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (!isSourceSet)
            {
                InitializeWithDefaults();
            }

            InitializeClientRpc();
        }
        

        [ClientRpc]
        private void InitializeClientRpc()
        {
            if (!IsServer)
            {
                Initialize();
            }
        }
        
        private void Initialize()
        {
            if (isInitialized) return;

            Setup();
            
            if (rb == null)
            {
                return;
            }
            
            Vector3 sourceVelocity = useSourceVelocity ? shooterVelocity : Vector3.zero;
            velocity = (transform.forward + direction) * (speed / 2) + sourceVelocity;
            rb.AddForce(velocity, ForceMode.VelocityChange);
            
            if (source != null && source.data != null)
            {
                maxVelocity = source.data.muzzleVelocity;
            }
            else
            {
                maxVelocity = speed; // Fallback value
            }

            maxVelocity = source.data.muzzleVelocity;

            if (transform.Find("Effects"))
            {
                Effects = transform.Find("Effects");
                Effects.parent = null;
            }

            DestroyBulletServerRpc();
            isInitialized = true;
        }
        
        private void InitializeWithDefaults()
        {
            // Initialize with default values when source data is unavailable
            maxVelocity = speed;
            // Set other necessary default values here
            isInitialized = true;
        }


        public virtual void Setup()
        {
            previousPosition = transform.position;
            startPosition = transform.position;

            transform.localScale = useAutoScaling ? Vector3.zero : Vector3.one * scaleMultipler;

            //FindComponents();
            rb = GetComponent<Rigidbody>();
            trail = GetComponentInChildren<TrailRenderer>();

            if (trail && useAutoScaling) trail.widthMultiplier = 0;
        }

        public virtual void FindComponents()
        {
            explosive = GetComponent<Explosive>();
        }


        private void Update()
        {
            if(!IsOwner) return;
            if (!IsSpawned || !isInitialized)
            {
                return;
            }
            
            float distanceFromStartPosition = Vector3.Distance(startPosition, transform.position);
            distanceFromStartPosition = Mathf.Clamp(distanceFromStartPosition, 0, range);

            damageRangeFactor = (rb.velocity.magnitude / maxVelocity) * (damageRangeCurve.Evaluate(distanceFromStartPosition / range));
            damage = (int)((!source.data.alwaysApplyFire ? source.data.damage / source.data.shotCount : source.data.damage) * damageRangeFactor);


            RaycastServerRpc();
            

            if (Effects)
            {
                Effects.position = transform.position;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RaycastServerRpc()
        {
            if (!IsSpawned || !isInitialized)
            {
                return;
            }

            PerformRaycast();
            
        }

        private void PerformRaycast()
        {
            Ray ray = new Ray(previousPosition, -(previousPosition - transform.position));
            RaycastHit[] hits = Physics.RaycastAll(ray, Vector3.Distance(transform.position, previousPosition));
            if (penetrationStrenght <= 0) DestroyBulletServerRpc();

            for (int i = 0; i < hits.Length; i++)
            {
                if (penetrationStrenght > 0)
                {
                    RaycastHit hit = hits[i];
                    UpdateHits(ray, hit);
                }
            }
            
            if (useAutoScaling)
            {
                float scale = (5 / scaleMultipler) * 1 / 6;

                transform.localScale = Vector3.one * scale;
                if (trail) trail.widthMultiplier = scale;
            }
            
            if (!useAutoScaling)
            {
                transform.localScale = Vector3.one * scaleMultipler;
            }
            
        }
        [ServerRpc(RequireOwnership = false)]
        private void DestroyBulletServerRpc()
        {
            if (IsServer)
                Destroy(gameObject, lifeTime);
        }
        
        private void FixedUpdate()
        {
            if (!IsSpawned || !isInitialized)
            {
                return;
            }

            if (rb != null)
            {
                rb.AddForce(Physics.gravity * gravity, ForceMode.Acceleration);
            }
        }

        private void LateUpdate()
        {
            previousPosition = transform.position;
        }

        private void UpdateHits(Ray ray, RaycastHit hit)
        {
            //stop if object has ignore component
            if (hit.transform.TryGetComponent(out IgnoreHitDetection ignore)) return;
            OnHit(hit);

            if (explosive)
            {
                explosive.Explode(true);
                //Destroy(gameObject);
                DestroyBulletServerRpc();
                return;
            }

            if (hit.transform.TryGetComponent(out CustomDecal customDecal))
            {
                penetrationStrenght -= customDecal.materialStrenght;
            }
            else
            {
                penetrationStrenght -= 10;
            }
            
            if (penetrationStrenght > 0)
                Demo.Scripts.Runtime.Item.Weapon.UpdateHits(source, this, defaultDecal, ray, hit, damage, damageRangeFactor, decalDirection);
            else
                DestroyBulletServerRpc();
        }

        public virtual void OnHit(RaycastHit hit)
        {

        }

        private void OnDestroy()
        {
            source?.Projectiles?.Remove(this);
        }
    }
}
