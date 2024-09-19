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
    private PlayerSpirit _playerSpirit;

    private void OnEnable()
    {
        _playerSpirit.playerHealthUpdate += PlayerHealthUpdateCurrentHealthTxt;
        _playerSpirit.playerHealthUpdate += UpdateHealthVisual;
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
        _playerSpirit.playerHealthUpdate -= PlayerHealthUpdateCurrentHealthTxt;
        _playerSpirit.playerHealthUpdate -= UpdateHealthVisual;
    }
}
