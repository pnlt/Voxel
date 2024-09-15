using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    public class RotateY : TransitionEffectBase<RectTransform>
    {
        public override void OnTransition(RectTransform rectTransform, float angle)
        {
            rectTransform.localRotation = Quaternion.Euler(new Vector3(rectTransform.localEulerAngles.x, angle,
                rectTransform.localRotation.z));
        }
    }
}