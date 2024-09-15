using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    public interface IOnExplosionHitInParent
    {
        void OnExplosionHitInParent(HitInfo hitInfo);
    }
}