using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Demo.Scripts.Runtime.Character;
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
    public GameObject miniMapUI;
    public TextMeshProUGUI currentHealthTxt;
    public Image healthVisual;

    public TextMeshProUGUI currentAmountTxt;
    public TextMeshProUGUI totalAmountTxt;
    //public float blinkIntensity;
    //public float blinkDuration;
    //float blinkTimer;
    //SkinnedMeshRenderer skinnedMeshRenderer;
    private static PlayerSpirit _instance;
    private void Awake()
    {
        currentHealth.Value = maxHealth;
        if(_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private void Start()
    {
        currentHealth.OnValueChanged += OnHealthChanged;
        if (IsOwner)
        {
            playerUI.SetActive(true);
            miniMapUI.SetActive(true);
        }
        else
        {
            playerUI.SetActive(false);
            miniMapUI.SetActive(false);
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

  //  private void Update()
  //  {
  //      blinkTimer -= Time.deltaTime;
  //      float lerp = Mathf.Clamp01(blinkTimer / blinkDuration);
  //      float intensity = (lerp * blinkIntensity) + 1.0f;
  //      //skinnedMeshRenderer.material.color = Color.white * intensity;
  //  }

    public void Die(ulong clientId)
    {
        if (m_IsPlayerDead)
        {
            Debug.Log("Die");
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
        //SpecialEffects.ScreenDamageEffect((float)damageValue / maxHealth);
        Die(clientId);

        //blinkTimer = blinkDuration;

        SpecialEffects.ScreenDamageEffect(Random.Range(0.1f, 1));
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



    //PHU NGUYEN

    public Material screenDamageMat;
    private Coroutine screenDamageTask;

    private void ScreenDamageEffect(float intensity)
    {
        if (screenDamageTask != null)
            StopCoroutine(screenDamageTask);

        screenDamageTask = StartCoroutine(screenDamage(intensity));
    }
    private IEnumerator screenDamage(float intensity)
    {
        // Screen Effect
        var targetRadius = Remap(intensity, 0, 1, 0.4f, -0.1f);
        var curRadius = 1f;
        for (float t = 0; curRadius != targetRadius; t += Time.deltaTime)
        {
            curRadius = Mathf.Clamp(Mathf.Lerp(1, targetRadius, t), 1, targetRadius);
            screenDamageMat.SetFloat("_Vignette_radius", curRadius);
            Debug.Log("log");
            yield return null;
        }
        for (float t = 0; curRadius < 1; t += Time.deltaTime)
        {
            curRadius = Mathf.Lerp(targetRadius, 1, t);
            screenDamageMat.SetFloat("_Vignette_radius", curRadius);
            Debug.Log("log");
            yield return null;
        }

    }

    private float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return Mathf.Lerp(toMin, toMax, Mathf.InverseLerp(fromMin, fromMax, value));
    }

    public static class SpecialEffects
    {
        public static void ScreenDamageEffect(float intensity) => _instance.ScreenDamageEffect(intensity);
    }
}
