#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(PressKeyEvent))]
    public class PressKeyEventEditor : Editor
    {
        private PressKeyEvent pkeTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            pkeTarget = (PressKeyEvent)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = DreamOSEditorHandler.GetDarkEditor(customSkin); }
            else { customSkin = DreamOSEditorHandler.GetLightEditor(customSkin); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "PKE Top Header");

            GUIContent[] toolbarTabs = new GUIContent[1];
            toolbarTabs[0] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 0;

            GUILayout.EndHorizontal();

            var hotkey = serializedObject.FindProperty("hotkey");
            var pressAnyKey = serializedObject.FindProperty("pressAnyKey");
            var pressAction = serializedObject.FindProperty("pressAction");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    DreamOSEditorHandler.DrawProperty(hotkey, customSkin, "Hotkey");
                    pressAnyKey.boolValue = DreamOSEditorHandler.DrawToggle(pressAnyKey.boolValue, customSkin, "Press Any Key");

                    DreamOSEditorHandler.DrawHeader(customSkin, "Events Header", 6);
                    EditorGUILayout.PropertyField(pressAction, new GUIContent("Press Key Events"), true);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif