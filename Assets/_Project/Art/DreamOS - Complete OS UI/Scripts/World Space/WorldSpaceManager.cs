using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Michsky.DreamOS
{
    public class WorldSpaceManager : MonoBehaviour
    {
        // Content
        public List<AudioSource> audioSources = new List<AudioSource>();

        // Resources
        public Camera mainCamera;
        public Camera projectorCam;
        public RawImage rendererImage;
        public Transform enterMount;
        public Canvas osCanvas;
        public FloatingIconManager useFloatingIcon;

        // Settings
        public bool requiresOpening = true;
        public bool autoGetIn = false;
        public bool lockCursorWhenOut = false;
        public bool dynamicRTSize = true;
        public int rtWidth = 1920;
        public int rtHeight = 1080;
        public string playerTag = "Player";
#if ENABLE_LEGACY_INPUT_MANAGER
        public KeyCode getInKey = KeyCode.E;
        public KeyCode getOutKey = KeyCode.Escape;
#elif ENABLE_INPUT_SYSTEM
        public InputAction getInKey;
        public InputAction getOutKey;
#endif
        [Range(1, 10)] public float audioBlendSpeed = 3;
        [Range(0.1f, 4)] public float transitionTime = 1f;
        public AnimationCurve transitionCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));
        public PositionMode positionMode = PositionMode.Local;

        // Events
        public UnityEvent onEnter;
        public UnityEvent onEnterEnd;
        public UnityEvent onExit;
        public UnityEvent onExitEnd;

        // Hidden & Helpers
        public int selectedTagIndex = 0;
        public bool isInSystem = false;
        bool isInTrigger = false;
        bool takenLocalRootPos;

        [HideInInspector] public RenderTexture uiRT;
        CanvasGroup osCG;
        Quaternion camRotHelper;
        Vector3 targetRootPos = new Vector3(0, 0, 0);

        public enum PositionMode { Local, World }

        void Awake()
        {
            if (dynamicRTSize == true) { uiRT = new RenderTexture(Screen.currentResolution.width, Screen.currentResolution.height, 24, RenderTextureFormat.RGB111110Float); }
            else { uiRT = new RenderTexture(rtWidth, rtHeight, 24, RenderTextureFormat.RGB111110Float); }

            if (projectorCam == null) 
            { 
                Debug.LogError("<b>[DreamOS]</b> Projector Camera is missing but it's essential.");
                return; 
            }

            projectorCam.targetTexture = uiRT;
            projectorCam.enabled = true;

            if (rendererImage != null) { rendererImage.texture = uiRT; }
            else { Debug.LogWarning("<b>[DreamOS]</b> Renderer Image is missing. The system will work but won't be rendered."); }

            osCG = osCanvas.GetComponent<CanvasGroup>();
            osCG.interactable = false;
            osCG.blocksRaycasts = false;

            if (requiresOpening == true) { osCanvas.gameObject.SetActive(false); }
            if (mainCamera == null) { mainCamera = Camera.main; }
        }

        void Update()
        {
            if (isInTrigger == false && isInSystem == true)
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                if (Input.GetKeyDown(getOutKey)) { TransitionOutHelper(); }
#elif ENABLE_INPUT_SYSTEM
                if (getOutKey.triggered) { TransitionOutHelper(); }
#endif
            }

            else if (isInSystem == false)
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                if (Input.GetKeyDown(getInKey)) { TransitionInHelper(); }
#elif ENABLE_INPUT_SYSTEM
                if (getInKey.triggered) { TransitionInHelper(); }
#endif
            }
        }

        public void EnableCamera(bool value) { mainCamera.enabled = value; }
        public void GetIn() { TransitionInHelper(); }
        public void GetOut() { TransitionOutHelper(); }

        void TransitionInHelper()
        {
            if (isInTrigger == false || isInSystem == true)
                return;

            // Events
            onEnter.Invoke();

            // Main CG
            osCG.interactable = true;
            osCG.blocksRaycasts = true;
            osCanvas.gameObject.SetActive(true);

            // Camera Position and Rotation
            if (positionMode == PositionMode.World) { targetRootPos = mainCamera.transform.position; }
            else if (positionMode == PositionMode.Local && takenLocalRootPos == false)
            {
                targetRootPos = mainCamera.transform.localPosition;
                takenLocalRootPos = true;
            }

            camRotHelper = mainCamera.transform.localRotation;

            // States
            isInTrigger = false;

            // Cursor
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Camera Transition
            StopCoroutine("CameraTransitionIn");
            StopCoroutine("CameraTransitionOut");
            StartCoroutine("CameraTransitionIn");

            // Audio
            // if (audioSources.Count != 0)
            // {
            //     StopCoroutine("BlendAudioSources2D");
            //     StopCoroutine("BlendAudioSources3D");
            //     StartCoroutine("BlendAudioSources2D");
            // }
        }

        void TransitionOutHelper()
        {
            // Events
            onExit.Invoke();

            // Main CG
            osCG.interactable = false;
            osCG.blocksRaycasts = false;

            // Rendering Stuff
            osCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            projectorCam.targetTexture = uiRT;
            projectorCam.enabled = true;

            // Cursor
            Cursor.visible = false;
            if (lockCursorWhenOut == true) { Cursor.lockState = CursorLockMode.Locked; }

            // Camera Transition
            StopCoroutine("CameraTransitionIn");
            StopCoroutine("CameraTransitionOut");
            StartCoroutine("CameraTransitionOut");

            // Audio
            // if (audioSources.Count != 0)
            // {
            //     StopCoroutine("BlendAudioSources2D");
            //     StopCoroutine("BlendAudioSources3D");
            //     StartCoroutine("BlendAudioSources3D");
            // }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == playerTag)
            {
                isInTrigger = true;
                if (autoGetIn == true) { TransitionInHelper(); }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == playerTag) { isInTrigger = false; }
        }

        IEnumerator CameraTransitionIn()
        {
            float elapsedTime = 0;
            Vector3 startingPos = mainCamera.transform.position;
            Quaternion startingRot = mainCamera.transform.rotation;

            while (elapsedTime < transitionTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
               
                mainCamera.transform.position = Vector3.Lerp(startingPos, enterMount.position, transitionCurve.Evaluate((elapsedTime / transitionTime)));
                mainCamera.transform.rotation = Quaternion.Slerp(startingRot, enterMount.rotation, transitionCurve.Evaluate((elapsedTime / transitionTime)));
               
                yield return null;
            }

            // Set Pos and Rot
            mainCamera.transform.position = enterMount.position;
            mainCamera.transform.rotation = enterMount.rotation;

            // Process Stuff
            osCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            projectorCam.enabled = false;
            isInSystem = true;
            onEnterEnd.Invoke();
        }

        IEnumerator CameraTransitionOut()
        {
            float elapsedTime = 0;
            Vector3 startingPos = mainCamera.transform.localPosition;
            Quaternion startingRot = mainCamera.transform.localRotation;

            while (elapsedTime < transitionTime)
            {
                elapsedTime += Time.unscaledDeltaTime;

                mainCamera.transform.localPosition = Vector3.Lerp(startingPos, targetRootPos, transitionCurve.Evaluate((elapsedTime / transitionTime)));
                mainCamera.transform.localRotation = Quaternion.Slerp(startingRot, camRotHelper, transitionCurve.Evaluate((elapsedTime / transitionTime)));

                yield return null;
            }

            // Set Pos and Rot
            mainCamera.transform.localPosition = targetRootPos;
            mainCamera.transform.localRotation = camRotHelper;

            // Process Stuff
            isInSystem = false;
            isInTrigger = true;
            onExitEnd.Invoke();
        }

        // IEnumerator BlendAudioSources2D()
        // {
        //     float elapsedTime = 0;
        //     float startinPoint = audioSources[0].spatialBlend;
        //
        //     while (audioSources[0].spatialBlend > 0.01)
        //     {
        //         foreach (AudioSource tempSource in audioSources) { tempSource.spatialBlend = Mathf.Lerp(startinPoint, 0, elapsedTime * audioBlendSpeed); }
        //         elapsedTime += Time.unscaledDeltaTime;
        //         yield return null;
        //     }
        //
        //     foreach (AudioSource tempSource in audioSources) { tempSource.spatialBlend = 0; }
        // }
        //
        // IEnumerator BlendAudioSources3D()
        // {
        //     float elapsedTime = 0;
        //     float startinPoint = audioSources[0].spatialBlend;
        //
        //     while (audioSources[0].spatialBlend < 0.99)
        //     {
        //         foreach (AudioSource tempSource in audioSources) { tempSource.spatialBlend = Mathf.Lerp(startinPoint, 1, elapsedTime * audioBlendSpeed); }
        //         elapsedTime += Time.unscaledDeltaTime;
        //         yield return null;
        //     }
        //
        //     foreach (AudioSource tempSource in audioSources) { tempSource.spatialBlend = 1; }
        // }
    }
}