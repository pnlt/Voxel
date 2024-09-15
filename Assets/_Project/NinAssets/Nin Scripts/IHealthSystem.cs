using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealthSystem
{
    void TakeDamage(float damage, PlayerSpirit.BodyPart position);
}
