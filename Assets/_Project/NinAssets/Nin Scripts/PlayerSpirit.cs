using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerSpirit : MonoBehaviour, IHealthSystem
{
    public enum BodyPart 
    {
        HEAD,
        BODY,
        LOWER_BODY
    }
    
    [SerializeField] private float maxHealth;
    private float currentHealth;
    public bool getShot;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        PlayerHealthUpdate?.Invoke(currentHealth);
    }

    public void TakeDamage(float damage, BodyPart position)
    {
        getShot = true;
        switch (position)
        {
            case BodyPart.HEAD:
                currentHealth -= maxHealth;
                break;
            case BodyPart.BODY:
                currentHealth -= damage;
                break;
            case BodyPart.LOWER_BODY:
                currentHealth -= damage * .5f;
                break;
        }

        GetShotEffect(.2f);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        PlayerHealthUpdate?.Invoke(currentHealth);
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
}
