using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    public interface IOnExplosionHitInChildren
    {
        void OnExplosionHitInChildren(HitInfo hitInfo);
    }
}