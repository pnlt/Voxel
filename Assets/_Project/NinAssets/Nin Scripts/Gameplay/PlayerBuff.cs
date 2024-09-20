using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerBuff : NetworkBehaviour
{
    //public Effect effect;
    public SecretBox secretBox;
    public GameObject GetVoxelUI;
    [SerializeField] private PlayerData data;
    
    private List<Effect> effects;

    private void Awake()
    {
        effects = new List<Effect>();
    }

    public void ApplyEffect(Effect newEffect)
    {
        newEffect.BuffEffect(data);
        effects.Add(newEffect);
    }

    private void RemoveEffect(Effect removedEffect)
    {
        effects.Remove(removedEffect);
    }
}