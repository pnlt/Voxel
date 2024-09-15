#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(GameHubLibrary))]
    public class GameHubLibraryEditor : Editor
    {
        private GUISkin customSkin;

        void OnEnable()
        {
            if (EditorGUIUtility.isProSkin == true) { customSkin = DreamOSEditorHandler.GetDarkEditor(customSkin); }
            else { customSkin = DreamOSEditorHandler.GetLightEditor(customSkin); }
        }

        public override void OnInspectorGUI()
        {
            // Games
            var games = serializedObject.FindProperty("games");

            DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 8);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel = 1;

            EditorGUILayout.PropertyField(games, new GUIContent("Game List"), true);

            games.isExpanded = true;
            EditorGUI.indentLevel = 0;

            if (GUILayout.Button("+  Add a new game", customSkin.button))
                games.arraySize += 1;

            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif