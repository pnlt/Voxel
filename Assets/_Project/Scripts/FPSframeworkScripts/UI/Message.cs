using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Akila.FPSFramework.UI
{
    [AddComponentMenu("Akila/FPS Framework/UI/Message")]
    public class Message : MonoBehaviour
    {
        public TextMeshProUGUI text;

        /// <summary>
        /// Sets the text's content.
        /// </summary>
        /// <param name="content"></param>
        public void SetMessage(string content)
        {
            text.SetText(content);
        }
    }
}