using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShootPlayer : MonoBehaviour
{
    public PlayerSpirit health;
    private Dictionary<int, PlayerSpirit.BodyPart> random = new();

    private void Start()
    {
        random.Add(1, PlayerSpirit.BodyPart.BODY);
        random.Add(2, PlayerSpirit.BodyPart.HEAD);
        random.Add(3, PlayerSpirit.BodyPart.LOWER_BODY);
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.N))
        {
            var partGetHurt = random[Random.Range(1, 4)];
            //health.TakeDamage(50, partGetHurt);
        }
    }
}
