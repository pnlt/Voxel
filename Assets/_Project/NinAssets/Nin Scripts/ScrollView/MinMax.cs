using System;

namespace InfimaGames.LowPolyShooterPack
{
    [Serializable]
    public struct MinMax
    {
        public float max;
        public float min;

        public MinMax(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}