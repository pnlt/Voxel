using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    public interface IOnHitInChildren
    {
        void OnHitInChildren(HitInfo hitInfo);
    }
}