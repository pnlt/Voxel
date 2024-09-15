using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/UI/Ping")]
    [RequireComponent(typeof(FloatingRect))]
    public class Ping : MonoBehaviour
    {
        public AudioProfile soundEffect;
    }
}