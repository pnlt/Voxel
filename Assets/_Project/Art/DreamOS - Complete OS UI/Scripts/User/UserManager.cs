using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

namespace Michsky.DreamOS
{
    public class UserManager : MonoBehaviour
    {
        // Resources
        public Animator setupScreen;
        public Animator transitScreen;
        public Animator lockScreen;
        public TMP_InputField lockScreenPassword;
        public BlurManager lockScreenBlur;
        public ProfilePictureLibrary ppLibrary;
        public GameObject ppItem;
        
        private Transform ppParent;
        private Animator desktopScreen;

        // Content
        [Range(1, 30)] public int minEmailCharacter = 1;
        [Range(1, 30)] public int maxEmailCharacter = 25;
        
        [Range(1, 20)] public int minUserNameCharacter = 1;
        [Range(1, 20)] public int maxUserNameCharacter = 14;

        [Range(1, 20)] public int minPasswordCharacter = 4;
        [Range(1, 20)] public int maxPasswordCharacter = 16;

        // Events
        public UnityEvent onLogin;
        public UnityEvent onLock;
        public UnityEvent onWrongPassword;

        // Settings
        public bool saveProfilePicture = true;
        public bool deletePrefsAtStart = false;
        public int ppIndex;

        // Multi Instance Support
        public bool allowMultiInstance;
        public string machineID = "DreamOS";

        // User variables
        [HideInInspector] public bool hasPassword;
        [HideInInspector] public bool emailOK;
        [HideInInspector] public bool userNameOK;
        [HideInInspector] public bool passwordOK;
        [HideInInspector] public bool passwordRetypeOK;
        [HideInInspector] public int userCreated;

        [HideInInspector] public bool isLockScreenOpen = false;

        [HideInInspector] public List<GetUserInfo> guiList = new List<GetUserInfo>();

        void Awake()
        {
            // Delete given prefs if option is enabled
            if (deletePrefsAtStart == true) { PlayerPrefs.DeleteAll(); }

            // Check for multi instance support
            if (allowMultiInstance == false) { machineID = "DreamOS"; }

            InitializeUserManager();
        }

        private void InitializeUserManager()
        {
            setupScreen.Play("Panel In");
        }

        public void TransitBetweenScreen(bool logToRegister)
        {
            if (logToRegister)
            {
                setupScreen.Play("Panel Out");
                transitScreen.Play("Panel In");
            }
            else
            {
                setupScreen.Play("Panel In");
                transitScreen.Play("Panel Out");
            }
        }

        public void InitializeProfilePictures()
        {
            if (ppParent == null || ppItem == null)
                return;

            foreach (Transform child in ppParent) { Destroy(child.gameObject); }
            for (int i = 0; i < ppLibrary.pictures.Count; ++i)
            {
                GameObject go = Instantiate(ppItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.transform.SetParent(ppParent, false);
                go.name = ppLibrary.pictures[i].pictureID;

                Image prevImage = go.transform.Find("Image Mask/Image").GetComponent<Image>();
                prevImage.sprite = ppLibrary.pictures[i].pictureSprite;

                Button wpButton = go.GetComponent<Button>();
                wpButton.onClick.AddListener(delegate 
                { 
                    //ChangeProfilePicture(go.transform.GetSiblingIndex()); 
                    //UpdateUserInfoUI();

                    try { wpButton.gameObject.GetComponentInParent<ModalWindowManager>().CloseWindow(); }
                    catch { Debug.Log("Cannot close the window due to missing modal window manager."); }
                });
            }

            GetAllUserInfoComps();
            //UpdateUserInfoUI();
        }

        // public void UpdateUserInfoUI()
        // {
        //     for (int i = 0; i < guiList.Count; ++i)
        //         guiList[i].GetInformation();
        // }

        public void GetAllUserInfoComps()
        {
            guiList.Clear();
            GetUserInfo[] list = FindObjectsOfType(typeof(GetUserInfo)) as GetUserInfo[];
            foreach (GetUserInfo obj in list) { guiList.Add(obj); }
        }


        public void BootSystem()
        {
            //bootManager.enabled = true;
            //bootManager.bootAnimator.gameObject.SetActive(true);
            setupScreen.gameObject.SetActive(false);
            //bootManager.bootAnimator.Play("Boot Start");
        }

        public void StartOS()
        {
            if (hasPassword == true)
            {
                lockScreenPassword.gameObject.SetActive(false);
                lockScreen.Play("Skip Login");
            }

            else
            {
                lockScreenPassword.gameObject.SetActive(true);
                lockScreen.Play("Lock Screen In");
            }
        }

        public void LockOS()
        {
            if (lockScreenBlur != null) { lockScreenBlur.BlurOutAnim(); }
            lockScreen.gameObject.SetActive(true);
            lockScreen.Play("Lock Screen In");
            //desktopScreen.Play("Desktop Out");
            onLock.Invoke();
        }

        public void LockScreenOpenClose()
        {
            if (isLockScreenOpen == true)
            {
                if (lockScreenBlur != null) { lockScreenBlur.BlurOutAnim(); }
                lockScreen.Play("Lock Screen Out");
            }
            else { lockScreen.Play("Lock Screen In"); }
        }

        private void OnDestroy()
        {
            PlayerPrefs.DeleteAll();
        }

        public void LockScreenAnimate()
        {
            if (hasPassword == true)
            {
                if (lockScreenBlur != null) { lockScreenBlur.BlurInAnim(); }
                lockScreen.Play("Lock Screen Password In");
            }

            else
            {
                if (lockScreenBlur != null) { lockScreenBlur.BlurOutAnim(); }
                lockScreen.Play("Lock Screen Out");
                //desktopScreen.Play("Desktop In");
                onLogin.Invoke();
            }
        }

        // public void Login()
        // {
        //     if (lockScreenPassword.text == password)
        //     {
        //         lockScreen.Play("Lock Screen Password Out");
        //         //desktopScreen.Play("Desktop In");
        //         onLogin.Invoke();
        //         StartCoroutine("DisableLockScreenHelper");
        //     }
        //     else if (lockScreenPassword.text != password) { onWrongPassword.Invoke(); }
        // }

        IEnumerator DisableLockScreenHelper()
        {
            yield return new WaitForSeconds(1f);
            lockScreen.gameObject.SetActive(false);
        }
    }
}