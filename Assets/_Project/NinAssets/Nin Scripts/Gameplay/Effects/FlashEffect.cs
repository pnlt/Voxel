using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this effect will help you run faster
[CreateAssetMenu (menuName = "Data/Effect/Flash be like")]
public class FlashEffect : Effect
{
    [SerializeField] private float runMultiplier;
    [SerializeField] private float runCooldown;
}
