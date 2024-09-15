using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    [CreateAssetMenu(fileName = "New Ammo Data", menuName = "Akila/FPS Framework/Weapons/Ammo Profile")]
    public class AmmoProfileData : ScriptableObject
    {
        public string Name;
    }
}