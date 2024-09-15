//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;
using System.Collections;
using Unity.Netcode;

namespace InfimaGames.LowPolyShooterPack.Legacy
{
	public class ImpactScript : NetworkBehaviour
	{

		[Header("Impact Despawn Timer")]
		//How long before the impact is destroyed
		public float despawnTimer = 10.0f;

		[Header("Audio")]
		public AudioClip[] impactSounds;

		public AudioSource audioSource;

		private void Start()
		{
			// Start the despawn timer
			StartCoroutine(DespawnTimer());

			//Get a random impact sound from the array
			audioSource.clip = impactSounds
				[Random.Range(0, impactSounds.Length)];
			//Play the random impact sound
			audioSource.Play();
		}

		private IEnumerator DespawnTimer()
		{
			//Wait for set amount of time
			yield return new WaitForSeconds(despawnTimer);
			// Call the ServerRpc to despawn the object
			DestroyImpactServerRpc();
		}
		[ServerRpc(RequireOwnership = false)]
		private void DestroyImpactServerRpc()
		{
			// Despawn the NetworkObject
			GetComponent<NetworkObject>().Despawn();
            
			// Destroy the game object after the delay
			Destroy(gameObject, despawnTimer);
		}
	}
}