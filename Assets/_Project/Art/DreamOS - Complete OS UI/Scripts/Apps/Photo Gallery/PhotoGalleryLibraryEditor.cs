#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(PhotoGalleryLibrary))]
    public class PhotoGalleryLibraryEditor : Editor
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
            var pictures = serializedObject.FindProperty("pictures");

            DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 8);
            GUILayout.BeginVertical();
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(pictures, new GUIContent("Picture List"), true);
            pictures.isExpanded = true;
            EditorGUI.indentLevel = 0;
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif