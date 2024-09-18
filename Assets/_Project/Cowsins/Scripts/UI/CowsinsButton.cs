using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

namespace cowsins
{

    [System.Serializable]
    public class CowsinsButton : Button
    {
        public enum ButtonType
        {
            SectionTransition,
            SceneTransition,
            Other
        }

        [SerializeField, Tooltip("Select the behaviour of the button.")] ButtonType buttonType;
        [SerializeField, Tooltip("GameObject to enable.You can add a CanvasGroup to this object to seamlessly play a fade in effect")] private GameObject sectionToEnable;
        [SerializeField, Tooltip("GameObjects to disable. No fade effect is played on these.")] private GameObject[] sectionsToDisable;
        [SerializeField, Tooltip("")] private AudioClip clickSFX;
        [SerializeField, Tooltip("")] private int sceneIndex;

        // Button Type Getter
        public ButtonType _ButtonType => buttonType;

        protected override void Start()
        {
            base.Start();
            // Run Button Click When the Button is Clicked
            this.onClick.AddListener(ButtonClick);
        }

        public void ButtonClick()
        {
            // Play sound
            MainMenuManager.Instance.PlaySound(clickSFX);

            // Handle Section Transitions
            if (buttonType == ButtonType.SectionTransition)
            {
                // Foreach section that we need to disable, disable it.
                foreach (GameObject section in sectionsToDisable) section.SetActive(false);
                // Gather the CanvasGroup component of the section we want to enable
                CanvasGroup canvasGroup = sectionToEnable.GetComponent<CanvasGroup>();
                // If the CanvasGroup exists begin animation
                if (canvasGroup)
                {
                    canvasGroup.alpha = 0;
                    MainMenuManager.Instance.SetObjectToLerp(canvasGroup);
                }
                // Enable the section
                sectionToEnable.SetActive(true);
            }
            else if (buttonType == ButtonType.SceneTransition) // Handle Scene Transitions
            {
                MainMenuManager.Instance.LoadScene(sceneIndex); // Load A new Scene (Async)
            }
        }

    }
#if UNITY_EDITOR
    [CustomEditor(typeof(CowsinsButton))]
    public class CowsinsButtonEditor : ButtonEditor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CowsinsButton myScript = target as CowsinsButton;
            EditorGUILayout.LabelField("COWSINS BUTTON", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clickSFX"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("buttonType"));
            if (myScript._ButtonType == CowsinsButton.ButtonType.SectionTransition)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("sectionToEnable"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("sectionsToDisable"));
            }
            else if (myScript._ButtonType == CowsinsButton.ButtonType.SceneTransition)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("sceneIndex"));
            }
            EditorGUILayout.Space(10);
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("UNITY BASE BUTTON", EditorStyles.boldLabel);
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();

        }
    }
#endif
}