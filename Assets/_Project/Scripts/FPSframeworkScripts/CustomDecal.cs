using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Audio System/Audio Low Pass Area")]
    public class CustomDecal : MonoBehaviour
    {
        public GameObject decalVFX;
        public float lifeTime = 60;
        public float materialStrenght = 10;
    }
}