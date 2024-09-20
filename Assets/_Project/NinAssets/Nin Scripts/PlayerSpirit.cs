using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Demo.Scripts.Runtime.Character;
using InfimaGames.LowPolyShooterPack.Assets_ăn_trộm._External_Assets.Infima_Games.Low_Poly_Shooter_Pack.Code.Client;
using KINEMATION.KAnimationCore.Runtime.Input;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using NetworkObject = Unity.Netcode.NetworkObject;
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

    public float blinkIntensity;
    public float blinkDuration;
    float blinkTimer;
    SkinnedMeshRenderer skinnedMeshRenderer;

    private void Awake()
    {
        currentHealth.Value = maxHealth;
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
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

    public void TakeDamage(int damageValue, BodyPart position, ulong clientId)
    {
        if (getShot) return;
        getShot = true;
        
        if (m_IsPlayerDead) return;
        
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
        //EffectsPN.SpecialEffects.ScreenDamageEffect((float)damageValue / maxHealth);
        Die(clientId);
        
        blinkTimer = blinkDuration;
        
        EffectsPN.SpecialEffects.ScreenDamageEffect(Random.Range(0.1f, 1));
    }

    private void Update()
    {
        blinkTimer -= Time.deltaTime;
        float lerp = Mathf.Clamp01(blinkTimer / blinkDuration);
        float intensity = (lerp * blinkIntensity) + 1.0f;
        skinnedMeshRenderer.material.color = Color.white * intensity;
    }

    public void Die(ulong clientId)
    {
        if (m_IsPlayerDead)
        {
            NetworkObject playerPrefab = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            playerPrefab.GetComponent<FPSMovement>().enabled = false;
            playerPrefab.GetComponent<FPSController>().enabled = false;    
            RespawnPlayer(clientId);
        }
    }
  
    public void RespawnPlayer(ulong clientId)
    {
        StartCoroutine(RespawnPlayerWithDelay(clientId, 5f));
    }
    private IEnumerator RespawnPlayerWithDelay(ulong clientId, float delay)
    {
        yield return new WaitForSeconds(delay); 

        RespawnPlayerServerRpc(clientId); 
    }
    [ServerRpc(RequireOwnership = false)]
    private void RespawnPlayerServerRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            NetworkObject playerPrefab = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            if (playerPrefab != null)
            {
                Vector3 respawnPosition = new Vector3(0f, 1f, 0f);
                playerPrefab.transform.position = respawnPosition;
                playerPrefab.GetComponent<FPSMovement>().enabled = true;
                playerPrefab.GetComponent<FPSController>().enabled = true;
                playerPrefab.GetComponent<PlayerSpirit>().currentHealth.Value = 100;
                RespawnPlayerClientRpc(clientId, respawnPosition);
            }
        }
        
    }
    [ClientRpc]
    private void RespawnPlayerClientRpc(ulong clientId, Vector3 respawnPosition)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            NetworkObject playerPrefab = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            if (playerPrefab != null)
            {
                playerPrefab.transform.position = respawnPosition;
                playerPrefab.GetComponent<FPSMovement>().enabled = true;
                playerPrefab.GetComponent<FPSController>().enabled = true;
            }
        }
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
