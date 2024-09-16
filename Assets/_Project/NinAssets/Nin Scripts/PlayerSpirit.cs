using System;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class PlayerSpirit : NetworkBehaviour, IHealthSystem
{
    public enum BodyPart 
    {
        HEAD,
        BODY,
        LOWER_BODY
    }
    
    [SerializeField] private float maxHealth;
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public bool getShot;

    private void Awake()
    {
        currentHealth.Value = maxHealth;
    }

    private void Start()
    {
        PlayerHealthUpdate?.Invoke(currentHealth.Value);
    }

    /*public override void OnNetworkSpawn()
    {
        currentHealth.OnValueChanged += (float prevVal, float newVal) =>
        {
            currentHealth.Value = newVal;
        };
    }*/

    [ServerRpc]
    private void TakeDamageServerRpc(float damage, BodyPart position)
    {
        Debug.Log("Rpc");
        getShot = true;
        switch (position)
        {
            case BodyPart.HEAD:
                currentHealth.Value -= maxHealth;
                if (IsHost)
                    Debug.Log("Server: Head");
                else if (IsClient)
                    Debug.Log("Client: Head");
                break;
            case BodyPart.BODY:
                currentHealth.Value -= damage * Random.Range(1f, 2f);
                currentHealth.Value = Mathf.RoundToInt(currentHealth.Value);
                if (IsHost)
                    Debug.Log("Server: body");
                else if (IsClient)
                    Debug.Log("Client: body");
                break;
            case BodyPart.LOWER_BODY:
                currentHealth.Value -= damage * .5f;
                if (IsHost)
                    Debug.Log("Server: lowerbody");
                else if (IsClient)
                    Debug.Log("Client: lowerbody");
                break;
        }

         GetShotEffect(.2f);
         currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);
         PlayerHealthUpdate?.Invoke(currentHealth.Value);
    }

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

    public static event Action<float> PlayerHealthUpdate;
    public void TakeDamage(float damage, BodyPart position)
    {
        TakeDamageServerRpc(damage, position);
    }
}
