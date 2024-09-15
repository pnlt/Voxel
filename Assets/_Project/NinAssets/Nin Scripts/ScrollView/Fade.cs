using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    public class Fade : TransitionEffectBase<CanvasGroup>
    {
        public override void OnTransition(CanvasGroup canvasGroup, float alpha)
        {
            canvasGroup.alpha = alpha;
        }
    }
}