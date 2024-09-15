﻿#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(MusicPlayerPlaylist))]
    public class MusicPlayerPlaylistEditor : Editor
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
            var coverImage = serializedObject.FindProperty("coverImage");
            var playlistName = serializedObject.FindProperty("playlistName");

            DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 8);
            DreamOSEditorHandler.DrawProperty(coverImage, customSkin, "Cover Image");
            DreamOSEditorHandler.DrawProperty(playlistName, customSkin, "Playlist Name");
            GUILayout.Space(18);


            // Content
            var playlist = serializedObject.FindProperty("playlist");

            DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 8);
            GUILayout.BeginVertical();
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(playlist, new GUIContent("Playlist Content"), true);
            playlist.isExpanded = true;
            EditorGUI.indentLevel = 0;
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif