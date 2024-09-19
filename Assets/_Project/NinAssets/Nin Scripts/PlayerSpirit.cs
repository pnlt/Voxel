using System;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerSpirit : NetworkBehaviour, IHealthSystem
{
    public enum BodyPart 
    {
        HEAD,
        BODY,
        LOWER_BODY
    }
    
    public int maxHealth = 100;
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>();
    private bool m_IsPlayerDead => currentHealth.Value == 0;
    public Action<PlayerSpirit> OnDie;
    public Action<float> playerHealthUpdate;
    public bool getShot;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
        
        // if (IsOwner)
        //     UpdateVisualClientRpc();
    }

    // private void Start()
    // {
    //     UpdateVisualServerRpc();
    // }

    // [ClientRpc]
    // private void UpdateVisualClientRpc()
    // {
    //     playerHealthUpdate.Invoke(currentHealth.Value);
    // }

    private async void GetShotEffect(float duration)
    {
        var playerPos = transform.position;
        var playerRotation = transform.eulerAngles;
        int multiplier = 1;
        
        float ellapse = 0;

        while (ellapse < duration)
        {
            var xRot = transform.eulerAngles.x + Random.Range(0.1f, 0.4f) * multiplier;
            if (xRot >= playerRotation.x + 0.8f)
                multiplier = -1;
            transform.eulerAngles = new Vector3(xRot, playerRotation.y, playerRotation.z);
            if (xRot <= playerRotation.x) 
                xRot = playerRotation.x;

            ellapse += Time.deltaTime;
            await UniTask.NextFrame();
        }

        getShot = false;
        transform.eulerAngles = playerRotation;
    }

    public void TakeDamage(int damageValue, BodyPart position)
    {
        if (getShot) return;
        getShot = true;
        
        if (m_IsPlayerDead)
        {
            return;
        }
        if (m_IsPlayerDead)
        {
            OnDie?.Invoke(this);
        }
        
        switch (position)
        {
            case BodyPart.HEAD:
                currentHealth.Value -= maxHealth;
                break;
            case BodyPart.BODY:
                currentHealth.Value -= damageValue * Random.Range(1, 3);
                currentHealth.Value = Mathf.RoundToInt(currentHealth.Value);
                break;
            case BodyPart.LOWER_BODY:
                currentHealth.Value -= damageValue * 1;
                currentHealth.Value = Mathf.RoundToInt(currentHealth.Value);
                break;
        }

        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);
        // if (IsOwner)
        //     UpdateVisualClientRpc();
        GetShotEffect(.2f);
    }
}
