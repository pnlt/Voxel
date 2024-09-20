using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

//this effect will help you run faster
[CreateAssetMenu (menuName = "Data/Effect/Flash be like")]
public class FlashEffect : Effect, IOutDateEffect
{
    [SerializeField] private float runMultiplier;
    [SerializeField] private float runCooldown;


    public override void BuffEffect(PlayerData stat)
    {
        //plus velocity
    }

    public async void UpdateEffectUse(PlayerData data)
    {
        while (true)
        {
            runCooldown -= Time.deltaTime;
            if (runCooldown <= 0)
            {
                StartCooldown(data).Forget();
                break;
            }
            await UniTask.Yield();
        }
    }
}