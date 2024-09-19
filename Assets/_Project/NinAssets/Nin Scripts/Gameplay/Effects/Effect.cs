using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class Effect : ScriptableObject
{
    [SerializeField] protected Sprite effectVisualization;
    [SerializeField] protected string name;
    [SerializeField] protected float cooldownTimeEffect;
    [SerializeField] protected string description;


    public bool _inCooldown;
    public abstract void BuffEffect(PlayerData stat);
    protected async UniTask StartCooldown(PlayerData data)
    {
        _inCooldown = true;
        var coolDown = cooldownTimeEffect;
        while (true)
        {
            coolDown -= Time.deltaTime;
            Debug.Log("cooldowning");
            if (coolDown <= 0)
            {
                _inCooldown = false;
                BuffEffect(data);
                break;
            }
        
            await UniTask.Yield();
        }
    }
    
    //public abstract void RestoreEffect()
}
