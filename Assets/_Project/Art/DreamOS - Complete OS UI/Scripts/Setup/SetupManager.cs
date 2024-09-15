using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

namespace Michsky.DreamOS
{
    public class SetupManager : MonoBehaviour
    {
        public bool isLogin;
        //Login steps list
        public StepItem loginProcess = new StepItem();
        //Register step List
        public List<StepItem> registrationSteps = new List<StepItem>();

        // Resources
        public UserManager userManager;
        [SerializeField] private Button loginNavBtn;
        [SerializeField] private Button registerNavBtn;
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField userNameInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private TMP_InputField passwordRetypeInput;
        [SerializeField] private Button infoContinueButton;
        [SerializeField] private Button privacyContinueButton;
        [SerializeField] private Animator errorMessageObject;
        [SerializeField] private TextMeshProUGUI errorMessageText;

        // Settings
        [SerializeField] private int currentPanelIndex = 0;
        [SerializeField] private bool enableBackgroundAnim = true;
        [SerializeField][TextArea] private string firstNameLengthError;
        [SerializeField][TextArea] private string lastNameLengthError;
        [SerializeField][TextArea] private string passwordLengthError;
        [SerializeField][TextArea] private string passwordRetypeError;

        private GameObject currentStep;
        private GameObject currentPanel;
        private GameObject nextPanel;
        private GameObject currentBG;
        private GameObject nextBG;

        [HideInInspector] public Animator currentStepAnimator;
        [HideInInspector] public Animator currentPanelAnimator;
        [HideInInspector] public Animator currentBGAnimator;
        [HideInInspector] public Animator nextPanelAnimator;
        [HideInInspector] public Animator nextBGAnimator;

        string panelFadeIn = "Panel In";
        string panelFadeOut = "Panel Out";
        string BGFadeIn = "Panel In";
        string BGFadeOut = "Panel Out";
        string stepFadeIn = "Check";

        [System.Serializable]
        public class StepItem
        {
            public string title = "Step";
            public GameObject indicator;
            public GameObject panel;
            public GameObject background;
            public StepContent stepContent;
        }

        public enum StepContent { Default, Information, Privacy }

        void Awake()
        {
            if (isLogin)
                LoginSystem();
            else
                RegisterSystem();
        }

        void Update()
        {
            if (userManager == null)
                return;
            
            //Login

            if (isLogin)
            {
                
            }
            else
            {
                //Registration
                if (registrationSteps[currentPanelIndex].stepContent == StepContent.Information)
                {
                    if (emailInput.text.Length >= userManager.minEmailCharacter &&
                        emailInput.text.Length <= userManager.maxEmailCharacter)
                    {
                        userManager.emailOK = true;

                        if (userNameInput.text.Length >= userManager.minUserNameCharacter &&
                            userNameInput.text.Length <= userManager.maxUserNameCharacter)
                        {
                            userManager.userNameOK = true;
                            infoContinueButton.interactable = true;

                            if (!errorMessageObject.GetCurrentAnimatorStateInfo(0).IsName("Out"))
                                errorMessageObject.Play("Out");
                        }
                        else
                        {
                            userManager.userNameOK = false;
                            infoContinueButton.interactable = false;
                            errorMessageText.text = lastNameLengthError;
                            LayoutRebuilder.ForceRebuildLayoutImmediate(errorMessageText.transform.parent
                                .GetComponent<RectTransform>());

                            if (!errorMessageObject.GetCurrentAnimatorStateInfo(0).IsName("In"))
                                errorMessageObject.Play("In");
                        }
                    }
                    else
                    {
                        userManager.emailOK = false;
                        infoContinueButton.interactable = false;
                        errorMessageText.text = firstNameLengthError;
                        LayoutRebuilder.ForceRebuildLayoutImmediate(errorMessageText.transform.parent
                            .GetComponent<RectTransform>());

                        if (!errorMessageObject.GetCurrentAnimatorStateInfo(0).IsName("In"))
                            errorMessageObject.Play("In");
                    }
                }
                else if (registrationSteps[currentPanelIndex].stepContent == StepContent.Privacy)
                {
                    if (passwordInput.text.Length >= userManager.minPasswordCharacter &&
                        passwordInput.text.Length <= userManager.maxPasswordCharacter || passwordInput.text.Length == 0)
                    {
                        userManager.passwordOK = true;

                        if (passwordInput.text != passwordRetypeInput.text)
                        {
                            userManager.passwordRetypeOK = false;
                            privacyContinueButton.interactable = false;
                            errorMessageText.text = passwordRetypeError;
                            LayoutRebuilder.ForceRebuildLayoutImmediate(errorMessageText.transform.parent
                                .GetComponent<RectTransform>());

                            if (!errorMessageObject.GetCurrentAnimatorStateInfo(0).IsName("In"))
                                errorMessageObject.Play("In");
                        }

                        else if (passwordInput.text == passwordRetypeInput.text)
                        {
                            userManager.passwordRetypeOK = true;
                            privacyContinueButton.interactable = true;

                            if (!errorMessageObject.GetCurrentAnimatorStateInfo(0).IsName("Out"))
                                errorMessageObject.Play("Out");
                        }
                    }
                    else
                    {
                        userManager.passwordOK = false;
                        privacyContinueButton.interactable = false;
                        errorMessageText.text = passwordLengthError;
                        LayoutRebuilder.ForceRebuildLayoutImmediate(errorMessageText.transform.parent
                            .GetComponent<RectTransform>());

                        if (!errorMessageObject.GetCurrentAnimatorStateInfo(0).IsName("In"))
                            errorMessageObject.Play("In");
                    }
                }
            }
        }

        #region Login

        private void LoginSystem()
        {
            currentPanel = loginProcess.panel;
            currentPanelAnimator = currentPanel.GetComponent<Animator>();
            
            if (currentPanelAnimator.transform.parent.gameObject.activeSelf == true)
            {
                currentPanelAnimator.Play(panelFadeIn);

                if (enableBackgroundAnim == true)
                {
                    currentBG = loginProcess.background;
                    currentBGAnimator = currentBG.GetComponent<Animator>();
                    currentBGAnimator.Play(BGFadeIn);
                }
                else
                {
                    currentBG = loginProcess.background;
                    currentBGAnimator = currentBG.GetComponent<Animator>();
                    currentBGAnimator.Play(BGFadeIn);
                }
            }
        }

        public void RegisterNavigation(GameObject loginPanel)
        {
            loginPanel.SetActive(false);
            RegisterSystem();
            isLogin = false;
        }

        #endregion

        #region Register

        private void RegisterSystem()
        {
            currentPanel = registrationSteps[currentPanelIndex].panel;
            currentPanelAnimator = currentPanel.GetComponent<Animator>();
            
            if (currentPanelAnimator.transform.parent.gameObject.activeSelf == true)
            {
                currentPanelAnimator.Play(panelFadeIn);

                if (enableBackgroundAnim == true)
                {
                    currentBG = registrationSteps[currentPanelIndex].background;
                    currentBGAnimator = currentBG.GetComponent<Animator>();
                    currentBGAnimator.Play(BGFadeIn);
                }
                else
                {
                    currentBG = registrationSteps[currentPanelIndex].background;
                    currentBGAnimator = currentBG.GetComponent<Animator>();
                    currentBGAnimator.Play(BGFadeIn);
                }
            }
        }

        public void LoginNavigation(GameObject loginPanel)
        {
            loginPanel.SetActive(true);
            LoginSystem();
            isLogin = true;
        }
        #endregion

        #region AnimationPanel 

        public void PanelAnim(int newPanel)
        {
            if (newPanel != currentPanelIndex)
            {
                currentPanel = registrationSteps[currentPanelIndex].panel;
                currentPanelIndex = newPanel;
                nextPanel = registrationSteps[currentPanelIndex].panel;

                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                nextPanelAnimator = nextPanel.GetComponent<Animator>();

                currentPanelAnimator.Play(panelFadeOut);
                nextPanelAnimator.Play(panelFadeIn);

                if (enableBackgroundAnim == true)
                {
                    currentBG = registrationSteps[currentPanelIndex].background;
                    currentPanelIndex = newPanel;
                    nextBG = registrationSteps[currentPanelIndex].background;

                    currentBGAnimator = currentBG.GetComponent<Animator>();
                    nextBGAnimator = nextBG.GetComponent<Animator>();

                    currentBGAnimator.Play(BGFadeOut);
                    nextBGAnimator.Play(BGFadeIn);
                }
            }
        }

        public void NextPage()
        {
            if (currentPanelIndex <= registrationSteps.Count - 2)
            {
                currentPanel = registrationSteps[currentPanelIndex].panel;
                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                currentPanelAnimator.Play(panelFadeOut);

                currentStep = registrationSteps[currentPanelIndex].indicator;
                currentStepAnimator = currentStep.GetComponent<Animator>();
                currentStepAnimator.Play(stepFadeIn);

                if (enableBackgroundAnim == true)
                {
                    currentBG = registrationSteps[currentPanelIndex].background;
                    currentBGAnimator = currentBG.GetComponent<Animator>();
                    currentBGAnimator.Play(BGFadeOut);
                }

                currentPanelIndex += 1;
                nextPanel = registrationSteps[currentPanelIndex].panel;

                nextPanelAnimator = nextPanel.GetComponent<Animator>();
                nextPanelAnimator.Play(panelFadeIn);

                if (enableBackgroundAnim == true)
                {
                    nextBG = registrationSteps[currentPanelIndex].background;
                    nextBGAnimator = nextBG.GetComponent<Animator>();
                    nextBGAnimator.Play(BGFadeIn);
                }
            }
        }

        public void PrevPage()
        {
            if (currentPanelIndex >= 1)
            {
                currentPanel = registrationSteps[currentPanelIndex].panel;
                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                currentPanelAnimator.Play(panelFadeOut);

                if (enableBackgroundAnim == true)
                {
                    currentBG = registrationSteps[currentPanelIndex].background;
                    currentBGAnimator = currentBG.GetComponent<Animator>();
                    currentBGAnimator.Play(BGFadeOut);
                }

                currentPanelIndex -= 1;
                nextPanel = registrationSteps[currentPanelIndex].panel;

                nextPanelAnimator = nextPanel.GetComponent<Animator>();
                nextPanelAnimator.Play(panelFadeIn);

                if (enableBackgroundAnim == true)
                {
                    nextBG = registrationSteps[currentPanelIndex].background;
                    nextBGAnimator = nextBG.GetComponent<Animator>();
                    nextBGAnimator.Play(BGFadeIn);
                }
            }
        }

        public void PlayLastStepAnim()
        {
            currentStep = registrationSteps[registrationSteps.Count].indicator;
            currentStepAnimator = currentStep.GetComponent<Animator>();
            currentStepAnimator.Play(stepFadeIn);
        }
        

        #endregion
    }
}