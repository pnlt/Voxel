using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : ScriptableObject
{
    [SerializeField] protected Sprite effectVisualization;
    [SerializeField] protected string name;
    [SerializeField] protected float cooldownTimeEffect;
    [SerializeField] protected string description;
}
