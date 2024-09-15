using UnityEngine;
using UnityEngine.Rendering;

namespace Michsky.DreamOS
{
    [ExecuteInEditMode]
    public class BlurChecker : MonoBehaviour
    {
        void OnEnable()
        {
            if (GraphicsSettings.renderPipelineAsset != null && gameObject.activeSelf == true)
                gameObject.SetActive(false);
        }
    }
}