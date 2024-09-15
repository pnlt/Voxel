#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(UserManager))]
    public class UserManagerEditor : Editor
    {
        private UserManager userTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            userTarget = (UserManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = DreamOSEditorHandler.GetDarkEditor(customSkin); }
            else { customSkin = DreamOSEditorHandler.GetLightEditor(customSkin); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "UM Top Header");

            GUIContent[] toolbarTabs = new GUIContent[3];
            toolbarTabs[0] = new GUIContent("Content");
            toolbarTabs[1] = new GUIContent("Resources");
            toolbarTabs[2] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Content", "Content"), customSkin.FindStyle("Tab Content")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 1;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 2;

            GUILayout.EndHorizontal();

            var errorMessage = serializedObject.FindProperty("errorMessage");
            var minNameCharacter = serializedObject.FindProperty("minUserNameCharacter");
            var maxNameCharacter = serializedObject.FindProperty("maxUserNameCharacter");
            var minEmailCharacter = serializedObject.FindProperty("minEmailCharacter");
            var maxEmailCharacter = serializedObject.FindProperty("maxEmailCharacter");
            var minPasswordCharacter = serializedObject.FindProperty("minPasswordCharacter");
            var maxPasswordCharacter = serializedObject.FindProperty("maxPasswordCharacter");
            var onLogin = serializedObject.FindProperty("onLogin");
            var onLock = serializedObject.FindProperty("onLock");
            var onWrongPassword = serializedObject.FindProperty("onWrongPassword");
            var setupScreen = serializedObject.FindProperty("setupScreen");
            var transitScreen = serializedObject.FindProperty("transitScreen");
            var lockScreen = serializedObject.FindProperty("lockScreen");
            var lockScreenPassword = serializedObject.FindProperty("lockScreenPassword");
            var lockScreenBlur = serializedObject.FindProperty("lockScreenBlur");
            var deletePrefsAtStart = serializedObject.FindProperty("deletePrefsAtStart");
            var showUserData = serializedObject.FindProperty("showUserData");
            var ppLibrary = serializedObject.FindProperty("ppLibrary");
            var ppItem = serializedObject.FindProperty("ppItem");
            var ppIndex = serializedObject.FindProperty("ppIndex");
            var saveProfilePicture = serializedObject.FindProperty("saveProfilePicture");

            switch (currentTab)
            {
                case 0:
                    Color defaultColor = GUI.color;
                    DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 6);

                    //Rule user name characters
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(new GUIContent("Min / Max User Name Char"), customSkin.FindStyle("Text"), GUILayout.Width(150));
                    GUILayout.BeginHorizontal();

                    minNameCharacter.intValue = EditorGUILayout.IntSlider(minNameCharacter.intValue, 0, maxNameCharacter.intValue - 1);
                    maxNameCharacter.intValue = EditorGUILayout.IntSlider(maxNameCharacter.intValue, minNameCharacter.intValue + 1, 20);

                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                        
                    //Rule email character
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(new GUIContent("Min / Max Email Char"), customSkin.FindStyle("Text"), GUILayout.Width(150));
                    GUILayout.BeginHorizontal();

                    minEmailCharacter.intValue = EditorGUILayout.IntSlider(minEmailCharacter.intValue, 0, maxEmailCharacter.intValue - 1);
                    maxEmailCharacter.intValue = EditorGUILayout.IntSlider(maxEmailCharacter.intValue, minEmailCharacter.intValue + 1, 30);

                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                        
                    //Rule password characters
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(new GUIContent("Min / Max Password Char"), customSkin.FindStyle("Text"), GUILayout.Width(150));
                    GUILayout.BeginHorizontal();

                    minPasswordCharacter.intValue = EditorGUILayout.IntSlider(minPasswordCharacter.intValue, 0, maxPasswordCharacter.intValue - 1);
                    maxPasswordCharacter.intValue = EditorGUILayout.IntSlider(maxPasswordCharacter.intValue, minPasswordCharacter.intValue + 1, 20);

                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();

                    DreamOSEditorHandler.DrawProperty(ppLibrary, customSkin, "Profile Picture Library");

                    GUILayout.EndHorizontal();

                    saveProfilePicture.boolValue = DreamOSEditorHandler.DrawToggle(saveProfilePicture.boolValue, customSkin, "Save Profile Picture");

                    if (userTarget.ppLibrary != null)
                    {
                        if (userTarget.ppLibrary.pictures.Count != 0)
                        {
                            GUILayout.Space(-3);
                            GUILayout.BeginHorizontal();
                            GUI.backgroundColor = Color.clear;

                            GUILayout.Box(TextureFromSprite(userTarget.ppLibrary.pictures[ppIndex.intValue].pictureSprite), GUILayout.Width(52), GUILayout.Height(52));

                            GUI.backgroundColor = defaultColor;
                            GUILayout.BeginVertical();
                            GUILayout.Space(4);
                            GUI.enabled = false;

                            EditorGUILayout.LabelField(new GUIContent("Selected Profile Picture"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                           
                            GUI.enabled = true;
                            GUILayout.Space(-4);

                            EditorGUILayout.LabelField(new GUIContent(userTarget.ppLibrary.pictures[ppIndex.intValue].pictureID), customSkin.FindStyle("Text"), GUILayout.Width(112));
                            GUILayout.Space(-4);
                            ppIndex.intValue = EditorGUILayout.IntSlider(ppIndex.intValue, 0, userTarget.ppLibrary.pictures.Count - 1);

                            GUILayout.Space(2);
                            GUILayout.EndVertical();
                            GUILayout.EndHorizontal();
                        }

                        else { EditorGUILayout.HelpBox("There is no item in the library.", MessageType.Warning); }
                    }

                    else { EditorGUILayout.HelpBox("Profile Picture Library is missing.", MessageType.Error); }

                    GUILayout.EndVertical();

                    DreamOSEditorHandler.DrawHeader(customSkin, "Events Header", 10);
                    EditorGUILayout.PropertyField(onLogin, new GUIContent("On Login"));
                    EditorGUILayout.PropertyField(onLock, new GUIContent("On Lock"));
                    EditorGUILayout.PropertyField(onWrongPassword, new GUIContent("On Wrong Password"));
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(setupScreen, customSkin, "Setup Screen");
                    DreamOSEditorHandler.DrawProperty(transitScreen, customSkin, "Transition Screen");
                    DreamOSEditorHandler.DrawProperty(lockScreen, customSkin, "Lock Screen");
                    DreamOSEditorHandler.DrawProperty(lockScreenPassword, customSkin, "Lock Screen Pass");
                    DreamOSEditorHandler.DrawProperty(lockScreenBlur, customSkin, "Lock Screen Blur");
                    DreamOSEditorHandler.DrawProperty(ppItem, customSkin, "PP Button");
                    break;

                case 2:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);

                    if (GUILayout.Button("Delete All Data", customSkin.button))
                    {
                        if (EditorUtility.DisplayDialog("Delete All PlayerPrefs Data", "Are you sure you want to delete all PlayerPrefs data? " +
                            "This action cannot be undone and will delete all PlayerPrefs data in your project.", "Yes", "No"))
                        {
                            PlayerPrefs.DeleteAll();
                        }
                    }

                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }

        public static Texture2D TextureFromSprite(Sprite sprite)
        {
            if (sprite == null) { return null; }

            if (sprite.rect.width != sprite.texture.width)
            {
                Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                             (int)sprite.textureRect.y,
                                                             (int)sprite.textureRect.width,
                                                             (int)sprite.textureRect.height);
                newText.SetPixels(newColors);
                newText.Apply();
                return newText;
            }

            else { return sprite.texture; }
        }
    }
}
#endif