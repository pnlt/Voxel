using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    public interface IOnHitInParent
    {
        void OnHitInParent(HitInfo hitInfo);
    }
}