using UnityEngine;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace cowsins
{
    public class GameSettingsManager : MonoBehaviour
    {
        [HideInInspector] public int fullScreen;
        [HideInInspector] public int res;
        [HideInInspector] public int maxFrameRate;
        [HideInInspector] public int vsync;
        [HideInInspector] public int graphicsQuality;
        [HideInInspector] public float masterVolume;
        [HideInInspector] public float playerSensX, playerSensY, playerControllerSensX, playerControllerSensY;

        [SerializeField] private TMP_Dropdown frameRateDropdown, resolutionRateDropdown, graphicsDropdown;
        [SerializeField] private Toggle fullScreenToggle, vsyncToggle;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider playerSensXSlider, playerSensYSlider, playerControllerSensXSlider, playerControllerSensYSlider;
        [SerializeField] private TextMeshProUGUI playerSensXDisplay, playerSensYDisplay, playerControllerSensXDisplay, playerControllerSensYDisplay;
        [SerializeField] private AudioMixer masterMixer;

        private int width = 1920, height = 1080;

        public static GameSettingsManager instance;

        private void Awake()
        {
            // We need to keep the new GameSettingsManager to keep the references
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(instance.gameObject);
                instance = this;
            }
            DontDestroyOnLoad(gameObject);
            LoadSettings();
            InitializeUI();
        }

        private void Update()
        {
            masterMixer.SetFloat("Volume", Mathf.Log10(masterVolume) * 20);
        }

        public void SetWindowedScreen() => fullScreen = 0;
        public void SetFullScreen() => fullScreen = 1;

        public void SaveSettings()
        {
            PlayerPrefs.SetInt("res", res);
            PlayerPrefs.SetInt("fullScreen", fullScreen);
            PlayerPrefs.SetInt("maxFrameRate", maxFrameRate);
            PlayerPrefs.SetInt("vsync", vsync);
            PlayerPrefs.SetInt("graphicsQuality", graphicsQuality);
            PlayerPrefs.SetFloat("masterVolume", masterVolume);
            PlayerPrefs.SetFloat("playerSensX", playerSensX);
            PlayerPrefs.SetFloat("playerSensY", playerSensY);
            PlayerPrefs.SetFloat("playerControllerSensX", playerControllerSensX);
            PlayerPrefs.SetFloat("playerControllerSensY", playerControllerSensY);
        }

        public void LoadSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("masterVolume", 1f);
            playerSensX = PlayerPrefs.GetFloat("playerSensX", 4f);
            playerSensY = PlayerPrefs.GetFloat("playerSensY", 4f);
            playerControllerSensX = PlayerPrefs.GetFloat("playerControllerSensX", 35f);
            playerControllerSensY = PlayerPrefs.GetFloat("playerControllerSensY", 35f);

            res = PlayerPrefs.GetInt("res", 0);
            fullScreen = PlayerPrefs.GetInt("fullScreen", 1);
            maxFrameRate = PlayerPrefs.GetInt("maxFrameRate", 0);
            vsync = PlayerPrefs.GetInt("vsync", 0);
            graphicsQuality = PlayerPrefs.GetInt("graphicsQuality", 2);

            ApplySettings();
            UpdateUIElements();
        }

        public void ResetSettings()
        {
            res = 0;
            fullScreen = 1;
            maxFrameRate = frameRateDropdown.options.Count - 1;
            vsync = 0;
            graphicsQuality = 2;
            masterVolume = 1;
            playerSensX = 4;
            playerSensY = 4;
            playerControllerSensX = 35;
            playerControllerSensY = 35;

            SaveSettings();
            LoadSettings();
        }

        private void InitializeUI()
        {
            frameRateDropdown.onValueChanged.AddListener(delegate { maxFrameRate = frameRateDropdown.value; });
            resolutionRateDropdown.onValueChanged.AddListener(delegate { res = resolutionRateDropdown.value; });
            graphicsDropdown.onValueChanged.AddListener(delegate { graphicsQuality = graphicsDropdown.value; });

            fullScreenToggle.onValueChanged.AddListener(delegate { fullScreen = fullScreenToggle.isOn ? 1 : 0; });
            vsyncToggle.onValueChanged.AddListener(delegate { vsync = vsyncToggle.isOn ? 1 : 0; });

            masterVolumeSlider.onValueChanged.AddListener(delegate
            {
                masterVolume = masterVolumeSlider.value;
                masterMixer.SetFloat("Volume", Mathf.Log10(masterVolume) * 20);
            });

            playerSensXSlider.onValueChanged.AddListener(delegate
            {
                playerSensX = playerSensXSlider.value;
                playerSensXDisplay.text = playerSensX.ToString("F1");
            });

            playerSensYSlider.onValueChanged.AddListener(delegate
            {
                playerSensY = playerSensYSlider.value;
                playerSensYDisplay.text = playerSensY.ToString("F1");
            });

            playerControllerSensXSlider.onValueChanged.AddListener(delegate
            {
                playerControllerSensX = playerControllerSensXSlider.value;
                playerControllerSensXDisplay.text = playerControllerSensX.ToString("F1");
            });

            playerControllerSensYSlider.onValueChanged.AddListener(delegate
            {
                playerControllerSensY = playerControllerSensYSlider.value;
                playerControllerSensYDisplay.text = playerControllerSensY.ToString("F1");
            });
        }

        private void ApplySettings()
        {
            switch (maxFrameRate)
            {
                case 0: Application.targetFrameRate = 60; break;
                case 1: Application.targetFrameRate = 120; break;
                case 2: Application.targetFrameRate = 230; break;
                case 3: Application.targetFrameRate = 300; break;
                default: Application.targetFrameRate = 60; break;
            }

            switch (res)
            {
                case 0:
                    width = 1920;
                    height = 1080;
                    break;
                case 1:
                    width = 1920;
                    height = 720;
                    break;
                case 2:
                    width = 854;
                    height = 480;
                    break;
                default:
                    width = 1920;
                    height = 1080;
                    break;
            }

            Screen.SetResolution(width, height, fullScreen == 1);
            QualitySettings.vSyncCount = vsync;
            QualitySettings.SetQualityLevel(graphicsQuality);
        }

        private void UpdateUIElements()
        {
            frameRateDropdown.value = maxFrameRate;
            resolutionRateDropdown.value = res;
            graphicsDropdown.value = graphicsQuality;
            fullScreenToggle.isOn = fullScreen == 1;
            vsyncToggle.isOn = vsync == 1;
            masterVolumeSlider.value = masterVolume;

            playerSensXSlider.value = playerSensX;
            playerSensYSlider.value = playerSensY;
            playerControllerSensXSlider.value = playerControllerSensX;
            playerControllerSensYSlider.value = playerControllerSensY;

            playerSensXDisplay.text = playerSensX.ToString("F1");
            playerSensYDisplay.text = playerSensY.ToString("F1");
            playerControllerSensXDisplay.text = playerControllerSensX.ToString("F1");
            playerControllerSensYDisplay.text = playerControllerSensY.ToString("F1");
        }

        private void OnSceneChanged(Scene current, Scene next)
        {
            PlayerPrefs.Save();
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }
    }
}
