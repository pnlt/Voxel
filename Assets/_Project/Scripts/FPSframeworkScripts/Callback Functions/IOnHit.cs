using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    public interface IOnHit
    {
        void OnHit(HitInfo hitInfo);
    }
}