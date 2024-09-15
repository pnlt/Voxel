using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    public interface IOnExplosionHit
    {
        void OnExplosionHit(HitInfo hitInfo);
    }
}