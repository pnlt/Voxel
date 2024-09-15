using System.Collections;
using UnityEngine;

namespace Michsky.DreamOS
{
    public class PopupPanelManager : MonoBehaviour
    {
        [Header("Settings")]
        public bool enableBlurAnim = true;
        public bool useTransition = true;
        public bool disableOnOut = true;
        public float closeOn = 25;
        public float panelSize = 100;

        [Header("Animation")]
        public DefaultState defaultPanelState;
        public AnimationDirection animationDirection;
        [SerializeField][Range(0.5f, 10)] private float curveSpeed = 3;
        [SerializeField][Range(1f, 12)] private float fadeSpeed = 3;
        public AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));
        public AnimationCurve fadeCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));

        RectTransform objectRect;
        CanvasGroup objectCG;
        BlurManager bManager;
        [HideInInspector] public bool isOn;

        public enum DefaultState { Minimized, Expanded }
        public enum AnimationDirection { Vertical, Horizontal }

        void Awake()
        {
            objectRect = gameObject.GetComponent<RectTransform>();

            if (useTransition == true)
            {
                objectCG = gameObject.GetComponent<CanvasGroup>();
                objectCG.alpha = 0;
                objectCG.interactable = false;
                objectCG.blocksRaycasts = false;
            }

            if (defaultPanelState == DefaultState.Minimized)
            {
                if (animationDirection == AnimationDirection.Vertical) { objectRect.sizeDelta = new Vector2(objectRect.sizeDelta.x, closeOn); }
                else { objectRect.sizeDelta = new Vector2(closeOn, objectRect.sizeDelta.y); }

                isOn = false;
            }

            else if (defaultPanelState == DefaultState.Expanded)
            {
                if (animationDirection == AnimationDirection.Vertical) { objectRect.sizeDelta = new Vector2(objectRect.sizeDelta.x, panelSize); }
                else { objectRect.sizeDelta = new Vector2(panelSize, objectRect.sizeDelta.y); }

                if (useTransition == true)
                {
                    objectCG.alpha = 1;
                    objectCG.interactable = true;
                    objectCG.blocksRaycasts = true;
                }

                isOn = true;
            }

            if (enableBlurAnim == true) { bManager = gameObject.GetComponent<BlurManager>(); }
            if (isOn == false && disableOnOut == true && defaultPanelState != DefaultState.Expanded) { gameObject.SetActive(false); }
        }

        public void AnimatePanel()
        {
            gameObject.SetActive(true);

            if (isOn == true)
            {
                if (useTransition == true) { objectCG.blocksRaycasts = false; objectCG.interactable = false; }
                if (enableBlurAnim == true) { bManager.BlurOutAnim(); }

                ClosePanel();
            }

            else if (isOn == false)
            {
                if (useTransition == true) { objectCG.blocksRaycasts = true; objectCG.interactable = true; }
                if (enableBlurAnim == true) { bManager.BlurInAnim(); }

                OpenPanel();
            }
        }

        public void OpenPanel()
        {
            if (objectRect == null)
                return;

            gameObject.SetActive(true);

            if (animationDirection == AnimationDirection.Horizontal)
            {
                StopCoroutine("HorizontalExpand");
                StopCoroutine("HorizontalMinimize");
                StartCoroutine("HorizontalExpand");
            }

            else if (animationDirection == AnimationDirection.Vertical)
            {
                StopCoroutine("VerticalExpand");
                StopCoroutine("VerticalMinimize");
                StartCoroutine("VerticalExpand");
            }

            if (useTransition == true) 
            {
                StopCoroutine("FadeIn");
                StopCoroutine("FadeOut");
                StartCoroutine("FadeIn");

                objectCG.blocksRaycasts = true; 
                objectCG.interactable = true; 
            }

            if (enableBlurAnim == true) 
            { 
                bManager.BlurInAnim();
            }

            isOn = true;
        }

        public void ClosePanel()
        {
            if (objectRect == null)
                return;

            if (animationDirection == AnimationDirection.Horizontal)
            {
                StopCoroutine("HorizontalExpand");
                StopCoroutine("HorizontalMinimize");
                StartCoroutine("HorizontalMinimize");
            }

            else if (animationDirection == AnimationDirection.Vertical)
            {
                StopCoroutine("VerticalExpand");
                StopCoroutine("VerticalMinimize");
                StartCoroutine("VerticalMinimize");
            }

            if (useTransition == true) 
            {
                StopCoroutine("FadeIn");
                StopCoroutine("FadeOut");
                StartCoroutine("FadeOut");

                StopCoroutine("CheckForDisable");
                StartCoroutine("CheckForDisable");

                objectCG.blocksRaycasts = false;
                objectCG.interactable = false;
            }

            if (enableBlurAnim == true) 
            { 
                bManager.BlurOutAnim();
            }

            isOn = false;
        }

        public void InstantMinimized()
        {
            if (objectRect == null || objectCG == null)
                return;

            objectRect.sizeDelta = new Vector2(objectRect.sizeDelta.x, closeOn);
            objectCG.alpha = 0;
        }

        public void InstantExpanded()
        {
            if (objectRect == null || objectCG == null)
                return;

            objectRect.sizeDelta = new Vector2(objectRect.sizeDelta.x, panelSize);
            objectCG.alpha = 1;
        }

        IEnumerator VerticalExpand()
        {
            float elapsedTime = 0;
            Vector2 startPos = objectRect.sizeDelta;
            Vector2 endPos = new Vector2(objectRect.sizeDelta.x, panelSize);

            while (objectRect.sizeDelta.y < panelSize - 0.1f)
            {
                elapsedTime += Time.unscaledDeltaTime;
                objectRect.sizeDelta = Vector2.Lerp(startPos, endPos, animationCurve.Evaluate(elapsedTime * curveSpeed));
                yield return null;
            }

            objectRect.sizeDelta = endPos;
        }

        IEnumerator VerticalMinimize()
        {
            float elapsedTime = 0;
            Vector2 startPos = objectRect.sizeDelta;
            Vector2 endPos = new Vector2(objectRect.sizeDelta.x, closeOn);

            while (objectRect.sizeDelta.y > closeOn + 0.1f)
            {
                elapsedTime += Time.unscaledDeltaTime;
                objectRect.sizeDelta = Vector2.Lerp(startPos, endPos, animationCurve.Evaluate(elapsedTime * curveSpeed));
                yield return null;
            }

            objectRect.sizeDelta = endPos;
        }

        IEnumerator HorizontalExpand()
        {
            float elapsedTime = 0;
            Vector2 startPos = objectRect.sizeDelta;
            Vector2 endPos = new Vector2(panelSize, objectRect.sizeDelta.y);

            while (objectRect.sizeDelta.y < panelSize - 0.1f)
            {
                elapsedTime += Time.unscaledDeltaTime;
                objectRect.sizeDelta = Vector2.Lerp(startPos, endPos, animationCurve.Evaluate(elapsedTime * curveSpeed));
                yield return null;
            }

            objectRect.sizeDelta = endPos;
        }

        IEnumerator HorizontalMinimize()
        {
            float elapsedTime = 0;
            Vector2 startPos = objectRect.sizeDelta;
            Vector2 endPos = new Vector2(closeOn, objectRect.sizeDelta.y);

            while (objectRect.sizeDelta.y > panelSize + closeOn)
            {
                elapsedTime += Time.unscaledDeltaTime;
                objectRect.sizeDelta = Vector2.Lerp(startPos, endPos, animationCurve.Evaluate(elapsedTime * curveSpeed));
                yield return null;
            }

            objectRect.sizeDelta = endPos;
        }

        IEnumerator FadeIn()
        {
            float elapsedTime = 0;
            float startValue = 0;

            while (objectCG.alpha < 0.99f)
            {
                elapsedTime += Time.unscaledDeltaTime;
                objectCG.alpha = Mathf.Lerp(startValue, 1, animationCurve.Evaluate(elapsedTime * fadeSpeed));
                yield return null;
            }

            objectCG.alpha = 1;
        }

        IEnumerator FadeOut()
        {
            float elapsedTime = 0;
            float startValue = objectCG.alpha;

            while (objectCG.alpha > 0.01f)
            {
                elapsedTime += Time.unscaledDeltaTime;
                objectCG.alpha = Mathf.Lerp(startValue, 0, animationCurve.Evaluate(elapsedTime * fadeSpeed));
                yield return null;
            }

            objectCG.alpha = 0;
        }

        IEnumerator CheckForDisable()
        {
            yield return new WaitForEndOfFrame();
            if (objectCG.alpha == 0) { StopCoroutine("CheckForDisable"); gameObject.SetActive(false); }
        }
    }
}