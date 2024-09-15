using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    public class Scale : TransitionEffectBase<RectTransform>
    {
        public override void OnTransition(RectTransform rectTransform, float scale)
        {
            rectTransform.localScale = Vector3.one * scale;
        }
    }
}