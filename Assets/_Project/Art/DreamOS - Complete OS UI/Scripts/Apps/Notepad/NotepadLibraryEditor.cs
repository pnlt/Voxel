#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(NotepadLibrary))]
    public class NotepadLibraryEditor : Editor
    {
        private GUISkin customSkin;

        void OnEnable()
        {
            if (EditorGUIUtility.isProSkin == true) { customSkin = DreamOSEditorHandler.GetDarkEditor(customSkin); }
            else { customSkin = DreamOSEditorHandler.GetLightEditor(customSkin); }
        }

        public override void OnInspectorGUI()
        {
            // Content
            var notes = serializedObject.FindProperty("notes");

            DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 8);
            GUILayout.BeginVertical();
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(notes, new GUIContent("Note List"), true);
            notes.isExpanded = true;
            EditorGUI.indentLevel = 0;
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif