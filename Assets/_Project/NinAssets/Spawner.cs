using InfimaGames.LowPolyShooterPack;
using InfimaGames.LowPolyShooterPack._Project.ScriptsPN;
using Unity.Netcode;
using UnityEngine;
using Weapon = Demo.Scripts.Runtime.Item.Weapon;

public class Spawner : NetworkBehaviour
{
    public static Spawner Instance;
    
    [Header ("Weapon")]
    public Weapon weapon;
    private WeaponData data;
    private Transform muzzle;
    private GameObject projectileParent;

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
        if (!IsOwner) return;
        
        SpawnBulletServerRpc();
    }

    [ServerRpc]
    private void SpawnBulletServerRpc()
    {
        CreateProjectile(data.projectile, weapon, muzzle, muzzle.forward, data.muzzleVelocity, data.range);
    }
    
    private void CreateProjectile(FPSProjectiles projectile, Weapon source, Transform muzzle, Vector3 direction, float speed, float range)
    {
        FPSProjectiles newProjectile = Instantiate(projectile, muzzle.position, muzzle.rotation);
        var networkObject = newProjectile.GetComponent<NetworkObject>();
        networkObject.Spawn(true);
    
        if (networkObject.IsSpawned)
        {
            AssignDataClientRpc(networkObject, direction, speed, range);
        }
        
    }

    // [ServerRpc]
    // private void AssignDataServerRpc(NetworkObjectReference projectileReference, Vector3 direction, float speed, float range)
    // {
    //     AssignDataClientRpc(projectileReference, direction, speed, range);   
    // }

    [ClientRpc]
    private void AssignDataClientRpc(NetworkObjectReference projectileReference, Vector3 direction, float speed, float range)
    {
        projectileReference.TryGet(out var projectileNetworkObj);
        var newProjectile = projectileNetworkObj.GetComponent<FPSProjectiles>();
        
        newProjectile.direction = direction;
        newProjectile.speed = speed;
        newProjectile.source = weapon;
        newProjectile.range = range;
        newProjectile.data = data;
        newProjectile.useAutoScaling = weapon.data.tracerRounds;
        newProjectile.scaleMultipler = weapon.data.projectileSize;
        newProjectile.damageRangeCurve = weapon.data.damageRangeCurve;
    }
}
