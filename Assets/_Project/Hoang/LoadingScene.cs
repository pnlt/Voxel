using cowsins;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{

    public Slider loadingSlider;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadingAsync());
    }

    IEnumerator LoadingAsync()
    {
        AsyncOperation lobbySceneOperation;
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (MainMenuManager.Instance.gameMode == 0)
            {
                lobbySceneOperation = SceneManager.LoadSceneAsync("lobby");

            }
            else
            {
                lobbySceneOperation = SceneManager.LoadSceneAsync("DemoRoom");
            }
        }
        else 
        {
            lobbySceneOperation = SceneManager.LoadSceneAsync("DessertMap");
        }
        lobbySceneOperation.allowSceneActivation = false;

        float targetLoadingTime = 4f;
        float elapsedTime = 0f;

        while (!lobbySceneOperation.isDone)
        {
            elapsedTime += Time.deltaTime;

            float timeBasedProgress = Mathf.Clamp01(elapsedTime / targetLoadingTime);
            float sceneProgress = Mathf.Clamp01(lobbySceneOperation.progress / 0.9f);

            float progressValue = Mathf.Min(timeBasedProgress, sceneProgress);
            loadingSlider.value = progressValue;

            if (sceneProgress >= 0.9f && elapsedTime >= targetLoadingTime)
            {
                loadingSlider.value = 1f;
                lobbySceneOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
