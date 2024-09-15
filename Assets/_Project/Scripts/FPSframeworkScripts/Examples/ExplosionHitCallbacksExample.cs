using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Examples/Explosion Hit Callbacks Example")]
    public class ExplosionHitCallbacksExample : MonoBehaviour, IOnExplosionHit, IOnExplosionHitInChildren, IOnExplosionHitInParent
    {
        public void OnExplosionHit(HitInfo info)
        {
            print("hit");
        }

        public void OnExplosionHitInChildren(HitInfo hitInfo)
        {
            print("hitInChildren");
        }

        public void OnExplosionHitInParent(HitInfo hitInfo)
        {
            print("hitInParent");
        }
    }
}