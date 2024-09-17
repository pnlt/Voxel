using System;
using System.Collections.Generic;
using InfimaGames.LowPolyShooterPack;
using InfimaGames.LowPolyShooterPack._Project.ScriptsPN;
using Unity.Netcode;
using UnityEngine;
using Weapon = Demo.Scripts.Runtime.Item.Weapon;

public class Spawner : NetworkBehaviour
{
    public static Spawner Instance;
    public WeaponData data;
    public Transform muzzle;
    public GameObject projectileParent;
    public Weapon weapon;

    //[SerializeField] private List<GameObject> spawnedProjectiles;

    // public override void OnNetworkSpawn()
    // {
    //     base.OnNetworkSpawn();
    //     spawnedProjectiles = new List<GameObject>();
    // }

    private void Awake()
    {
        Instance = this;
    }
    
    public void SetData(WeaponData data, Transform muzzle, Weapon weapon)
    {
        this.data = data;
        this.muzzle = muzzle;
        this.weapon = weapon;
    }

    public void SpawnBullet()
    {
        SpawnBulletServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBulletServerRpc()
    {
        //SpawnBulletClientRpc();
        CreateProjectile(data.projectile, weapon, muzzle, muzzle.forward, data.muzzleVelocity, data.range);
    }

    /*[ClientRpc]
    private void SpawnBulletClientRpc()
    {
        FPSProjectiles projectile;
        projectile = CreateProjectile(data.projectile, weapon, muzzle, muzzle.forward, data.muzzleVelocity, data.range);
        
        var projectileOnNetwork = projectile.GetComponent<NetworkObject>();
        projectileOnNetwork.Spawn(true);
    }*/
    
    private void CreateProjectile(FPSProjectiles projectile, Weapon source, Transform muzzle, Vector3 direction, float speed, float range)
    {
        FPSProjectiles newProjectile = Instantiate(projectile, muzzle.position, muzzle.rotation);
        //spawnedProjectiles.Add(newProjectile.gameObject);
        //newProjectile.GetComponent<FPSProjectiles>().parent = this;
        
        var projectileOnNetwork = newProjectile.GetComponent<NetworkObject>();
        projectileOnNetwork.Spawn(true);
        //if (_fpsController && _fpsController.characterController)
            //newProjectile.shooterVelocity = _fpsController.characterController.velocity;

        AssignDataServerRpc(projectileOnNetwork.GetComponent<NetworkObject>(), direction, speed, range);

        source.Projectiles?.Add(newProjectile);
    }

    // [ServerRpc(RequireOwnership = false)]
    // public void DestroyServerRpc()
    // {
    //     GameObject toDestroy = spawnedProjectiles[0];
    //     toDestroy.GetComponent<NetworkObject>().Despawn();
    //     spawnedProjectiles.Remove(toDestroy);
    //     Destroy(toDestroy);
    // }

    [ServerRpc]
    private void AssignDataServerRpc(NetworkObjectReference projectileReference, Vector3 direction, float speed, float range)
    {
        AssignDataClientRpc(projectileReference, direction, speed, range);   
    }

    [ClientRpc]
    private void AssignDataClientRpc(NetworkObjectReference projectileReference, Vector3 direction, float speed, float range)
    {
        projectileReference.TryGet(out var projectileNetworkObj);
        var newProjectile = projectileNetworkObj.GetComponent<FPSProjectiles>();
        
        newProjectile.direction = direction;
        newProjectile.speed = speed;
        newProjectile.source = weapon;
        newProjectile.range = range;
        newProjectile.useAutoScaling = weapon.data.tracerRounds;
        newProjectile.scaleMultipler = weapon.data.projectileSize;
        newProjectile.damageRangeCurve = weapon.data.damageRangeCurve;
    }
}
