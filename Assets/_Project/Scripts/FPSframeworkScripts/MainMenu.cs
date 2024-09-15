using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/UI/MainMenu")]
    public class MainMenu : MonoBehaviour
    {
        private void Awake()
        {
            if (!LoadingScreen.Instance)
                SceneManager.LoadSceneAsync("Loading", LoadSceneMode.Additive);
        }

        public void LoadGame(string name)
        {
            StartCoroutine(LoadingScreen.Instance.LoadSceneAsync(name));
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}