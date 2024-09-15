using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Utility/FrameRateLimiter")]
    public class FrameRateLimiter : MonoBehaviour
    {
        public int frameRate = 60;

        private void Update()
        {
            Application.targetFrameRate = frameRate;
        }
    }
}