using System.Collections;
using System.Collections.Generic;
using Akila.FPSFramework;
using InfimaGames.LowPolyShooterPack._Project.ScriptsPN;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    public class FPSHitInfo 
    {
        public Ray ray { get; set; }
        public RaycastHit raycastHit { get; set; }

        /// <summary>
        /// If hit was done from a projectile this will be the source. Use this to get data about the hit.
        /// </summary>
        public FPSProjectiles projectile;

        /// <summary>
        /// If hit was done from an explosive this will be the source. Use this to get data about the hit.
        /// </summary>
        public Explosive explosive;

        public FPSHitInfo(FPSProjectiles projectile, Ray ray, RaycastHit raycastHit)
        {
            this.ray = ray;
            this.raycastHit = raycastHit;
            this.projectile = projectile;
        }

        public FPSHitInfo(Explosive explosive, Ray ray, RaycastHit raycastHit)
        {
            this.ray = ray;
            this.raycastHit = raycastHit;
            this.explosive = explosive;
        }
    }
}
