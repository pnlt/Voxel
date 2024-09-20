using System;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
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
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    private bool m_IsPlayerDead => currentHealth.Value == 0;
    public Action<PlayerSpirit> OnDie;
    public bool getShot;

    public GameObject playerUI;
    public TextMeshProUGUI currentHealthTxt;
    public Image healthVisual;

    public TextMeshProUGUI currentAmountTxt;
    public TextMeshProUGUI totalAmountTxt;

    private void Awake()
    {
        currentHealth.Value = maxHealth;
    }

    private void Start()
    {
        currentHealth.OnValueChanged += OnHealthChanged;
        if (IsOwner)
        {
            playerUI.SetActive(true);
        }
        else
        {
            playerUI.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int previousHealth, int newHealth)
    {
        PlayerHealthUpdateCurrentHealthTxtClientRpc(newHealth);
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
                currentHealth.Value -= damageValue * Random.Range(1, 2);
                break;
            case BodyPart.LOWER_BODY:
                currentHealth.Value -= damageValue * 1;
                break;
        }
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);
        PlayerHealthUpdateCurrentHealthTxtClientRpc(currentHealth.Value);
        GetShotEffect(.2f);
        
    }

    [ClientRpc]
    public void PlayerHealthUpdateCurrentHealthTxtClientRpc(int currentHealth)
    {
        if (IsOwner)
        {
            currentHealthTxt.text = currentHealth.ToString();
            healthVisual.fillAmount = currentHealth / 100f;
        }
    }

    public void UpdateCurrentAmountTxt(int currentAmount)
    {
        currentAmountTxt.text = currentAmount.ToString();
    }

    public void UpdateTotalAmountTxt(int totalAmount)
    {
        totalAmountTxt.text = totalAmount.ToString();
    }
}
