#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(VideoPlayerLibrary))]
    public class VideoPlayerLibraryEditor : Editor
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
            var videos = serializedObject.FindProperty("videos");

            DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 8);
            GUILayout.BeginVertical();
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(videos, new GUIContent("Video List"), true);
            videos.isExpanded = true;
            EditorGUI.indentLevel = 0;
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif