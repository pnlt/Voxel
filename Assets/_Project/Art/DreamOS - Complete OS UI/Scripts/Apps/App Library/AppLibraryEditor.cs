﻿#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(AppLibrary))]
    public class AppLibraryEditor : Editor
    {
        private GUISkin customSkin;

        void OnEnable()
        {
            if (EditorGUIUtility.isProSkin == true) { customSkin = DreamOSEditorHandler.GetDarkEditor(customSkin); }
            else { customSkin = DreamOSEditorHandler.GetLightEditor(customSkin); }
        }

        public override void OnInspectorGUI()
        {
            // Settings
            var alwaysUpdate = serializedObject.FindProperty("alwaysUpdate");
            var optimizeUpdates = serializedObject.FindProperty("optimizeUpdates");

            DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 8);
            alwaysUpdate.boolValue = DreamOSEditorHandler.DrawToggle(alwaysUpdate.boolValue, customSkin, "Always Update");
            optimizeUpdates.boolValue = DreamOSEditorHandler.DrawToggle(optimizeUpdates.boolValue, customSkin, "Optimize Update");

            // Apps
            var apps = serializedObject.FindProperty("apps");

            DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 8);
            GUILayout.BeginVertical();
            EditorGUI.indentLevel = 1;

            EditorGUILayout.PropertyField(apps, new GUIContent("App List"), true);
            apps.isExpanded = true;

            EditorGUI.indentLevel = 0;
            GUILayout.EndVertical();
     
            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif