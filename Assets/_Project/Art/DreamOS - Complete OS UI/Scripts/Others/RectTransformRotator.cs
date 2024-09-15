using UnityEngine;

namespace Michsky.DreamOS
{
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformRotator : MonoBehaviour
    {
        [Header("Resources")]
        public RectTransform objectToRotate;

        [Header("Settings")]
        public float multiplier = 15;

        float currentPos;

        void Awake()
        {
            if (objectToRotate == null)
            {
                objectToRotate = GetComponent<RectTransform>();
            }
        }

        void Update()
        {
            currentPos += multiplier * Time.deltaTime;
            objectToRotate.localRotation = Quaternion.Euler(0, 0, currentPos);

            if (currentPos >= 360)
            {
                currentPos = 0;
            }
        }
    }
}