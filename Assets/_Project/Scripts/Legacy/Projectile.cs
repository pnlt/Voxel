//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;
using System.Collections;
using InfimaGames.LowPolyShooterPack;
using Unity.Netcode;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

namespace InfimaGames.LowPolyShooterPack.Legacy
{
	public class Projectile : NetworkBehaviour
	{
		public float damage;
		public Weapon parent;

		[Range(5, 100)]
		[Tooltip("After how long time should the bullet prefab be destroyed?")]
		public float destroyAfter;

		[Tooltip("If enabled the bullet destroys on impact")]
		public bool destroyOnImpact = false;

		[Tooltip("Minimum time after impact that the bullet is destroyed")]
		public float minDestroyTime;

		[Tooltip("Maximum time after impact that the bullet is destroyed")]
		public float maxDestroyTime;

		[Header("Impact Effect Prefabs")]
		[SerializeField] private GameObject[] bloodImpactPrefabs;
		[SerializeField] private GameObject[] metalImpactPrefabs;
		[SerializeField] private GameObject[] dirtImpactPrefabs;
		[SerializeField] private GameObject[] concreteImpactPrefabs;

		private void Start()
		{
			//Grab the game mode service, we need it to access the player character!
			var gameModeService = ServiceLocator.Current.Get<IGameModeService>();
			//Ignore the main player character's collision. A little hacky, but it should work.
			Physics.IgnoreCollision(gameModeService.GetPlayerCharacter().GetComponent<Collider>(),
				GetComponent<Collider>());

			//Start destroy timer
			StartCoroutine(DestroyAfter());
		}

		//If the bullet collides with anything
		private void OnCollisionEnter(Collision collision)
		{
			if (!IsOwner) return;
			//Ignore collisions with other projectiles.
			if (collision.gameObject.GetComponent<Projectile>() != null)
				return;

			Vector3 position = transform.position;
			Vector3 normal = collision.contacts[0].normal;

			//coroutine with random destroy timer
			if (destroyOnImpact)
			{
				parent.DestroyServerRpc();
				return;
			}
			//Otherwise, destroy bullet on impact
			else
			{
				StartCoroutine(DestroyTimer());
			}
			
			// Determine the type of collision and handle accordingly
			switch (collision.transform.tag)
			{
				case "Blood":
					HandleBloodImpactServerRpc(position, normal);
					break;
				case "Metal":
					HandleMetalImpactServerRpc(position, normal);
					break;
				case "Dirt":
					HandleDirtImpactServerRpc(position, normal);
					break;
				case "Concrete":
					HandleConcreteImpactServerRpc(position, normal);
					break;
				default:
					// Handle other types of collisions or do nothing
					break;
			}

			//If bullet collides with "Target" tag
			if (collision.transform.CompareTag("Target"))
			{
				//Toggle "isHit" on target object
				collision.transform.gameObject.GetComponent
					<TargetScript>().isHit = true;
				//Destroy bullet object
				parent.DestroyServerRpc();
			}
			//If bullet collides with "ExplosiveBarrel" tag
			else if (collision.transform.CompareTag("ExplosiveBarrel"))
			{
				//Toggle "explode" on explosive barrel object
				collision.transform.gameObject.GetComponent
					<ExplosiveBarrelScript>().explode = true;
				//Destroy bullet object
				parent.DestroyServerRpc();
			}

			//If bullet collides with "GasTank" tag
			else if (collision.transform.CompareTag("GasTank"))
			{
				//Toggle "isHit" on gas tank object
				collision.transform.gameObject.GetComponent
					<GasTankScript>().isHit = true;
				//Destroy bullet object
				parent.DestroyServerRpc();
			}
		}
		
		[ServerRpc]
		private void HandleBloodImpactServerRpc(Vector3 position, Vector3 normal)
		{
			GameObject bloodImpact = Instantiate(bloodImpactPrefabs[Random.Range(0, bloodImpactPrefabs.Length)], position, Quaternion.LookRotation(normal));
			NetworkObject networkObject = bloodImpact.GetComponent<NetworkObject>();
			networkObject.Spawn();
			parent.DestroyServerRpc();
		}

		[ServerRpc]
		private void HandleMetalImpactServerRpc(Vector3 position, Vector3 normal)
		{
			GameObject metalImpact = Instantiate(metalImpactPrefabs[Random.Range(0, metalImpactPrefabs.Length)], position, Quaternion.LookRotation(normal));
			NetworkObject networkObject = metalImpact.GetComponent<NetworkObject>();
			networkObject.Spawn();
			parent.DestroyServerRpc();
		}

		[ServerRpc]
		private void HandleDirtImpactServerRpc(Vector3 position, Vector3 normal)
		{
			GameObject dirtImpact = Instantiate(dirtImpactPrefabs[Random.Range(0, dirtImpactPrefabs.Length)], position, Quaternion.LookRotation(normal));
			NetworkObject networkObject = dirtImpact.GetComponent<NetworkObject>();
			networkObject.Spawn();
			parent.DestroyServerRpc();
		}

		[ServerRpc]
		private void HandleConcreteImpactServerRpc(Vector3 position, Vector3 normal)
		{
			GameObject concreteImpact = Instantiate(concreteImpactPrefabs[Random.Range(0, concreteImpactPrefabs.Length)], position, Quaternion.LookRotation(normal));
			NetworkObject networkObject = concreteImpact.GetComponent<NetworkObject>();
			networkObject.Spawn();
			parent.DestroyServerRpc();
		}

		private IEnumerator DestroyTimer()
		{
			//Wait random time based on min and max values
			yield return new WaitForSeconds
				(Random.Range(minDestroyTime, maxDestroyTime));
			//Destroy bullet object
				parent.DestroyServerRpc();
		}

		private IEnumerator DestroyAfter()
		{
			//Wait for set amount of time
			yield return new WaitForSeconds(destroyAfter);
			//Destroy bullet object
				parent.DestroyServerRpc();
		}
	}
}