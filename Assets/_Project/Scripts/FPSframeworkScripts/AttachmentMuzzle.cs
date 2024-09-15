using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Weapons/Attachments/Muzzle")]
    public class AttachmentMuzzle : MonoBehaviour
    {
        public AudioProfile fireSFX;
        public ParticleSystem[] muzzleEffects;
    }
}