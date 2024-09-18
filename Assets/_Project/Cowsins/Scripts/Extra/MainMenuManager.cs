using UnityEngine;
using UnityEngine.SceneManagement;

namespace cowsins
{
    public class MainMenuManager : MonoBehaviour
    {
        public static MainMenuManager Instance { get; private set; }

        [SerializeField, Header("Main")] private CanvasGroup introMenu, mainMenu;

        private CanvasGroup objectToLerp;

        private AudioSource audioSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            introMenu.gameObject.SetActive(true);
            introMenu.alpha = 1;

            mainMenu.gameObject.SetActive(false);
            mainMenu.alpha = 0;

            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (!objectToLerp) return;
            objectToLerp.gameObject.SetActive(true);
            objectToLerp.alpha += Time.deltaTime * 3;
        }


        public void SetObjectToLerp(CanvasGroup To) => objectToLerp = To;

        public void ChangeScene(int scene) => SceneManager.LoadScene(scene);

        public void PlaySound(AudioClip clickSFX)
        {
            audioSource.clip = clickSFX;
            audioSource.Play();
        }

        public void LoadScene(int sceneIndex)
        {
            SceneManager.LoadSceneAsync(sceneIndex);
        }

    }
}
