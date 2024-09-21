using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealthSystem
{
    void TakeDamage(int damageValue, PlayerSpirit.BodyPart position, ulong clientID);
}
