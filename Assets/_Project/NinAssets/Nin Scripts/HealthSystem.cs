using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    public TextMeshProUGUI currentHealth;
    public Image healthVisual;

    private void OnEnable()
    {
        PlayerSpirit.PlayerHealthUpdate += PlayerHealthUpdateCurrentHealthTxt;
        PlayerSpirit.PlayerHealthUpdate += UpdateHealthVisual;
    }

    private void PlayerHealthUpdateCurrentHealthTxt(float currentHealth)
    {
        this.currentHealth.text = currentHealth.ToString();
    }

    private void UpdateHealthVisual(float currentHealth)
    {
        healthVisual.fillAmount = currentHealth / 100f;
    }

    private void OnDisable()
    {
        PlayerSpirit.PlayerHealthUpdate -= PlayerHealthUpdateCurrentHealthTxt;
        PlayerSpirit.PlayerHealthUpdate -= UpdateHealthVisual;
    }
}
