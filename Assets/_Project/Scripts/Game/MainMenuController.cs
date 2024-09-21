using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _submitCodeButton;
        [SerializeField] private TextMeshProUGUI _codeText;

        private void OnEnable()
        {
            _hostButton.onClick.AddListener(OnHostClicked);
            _submitCodeButton.onClick.AddListener(OnSubmitCodeClicked);
        }

        private void OnDisable()
        {
            _hostButton.onClick.RemoveListener(OnHostClicked);
            _submitCodeButton.onClick.RemoveListener(OnSubmitCodeClicked);
        }

        private async void OnLeaveGameClicked()
        {
           bool succeeded = await GameLobbyManager.Instance.LeaveAllLobby(); 
           if (succeeded)
           {
               //Debug.Log("All lobbies left");
           }
        }

        private async void OnRejoinGameClicked()
        {
          bool succeeded = await GameLobbyManager.Instance.RejoinGame();
          if (succeeded)
          {
                SceneManager.LoadSceneAsync("Lobby");
          }
        }

        private async void OnHostClicked()
        {
            bool succeeded = await GameLobbyManager.Instance.CreateLobby();
            if (succeeded)
            {
                StartCoroutine(LoadingAsync());
            }
        }

        IEnumerator LoadingAsync()
        {
            SceneManager.LoadSceneAsync("Loading", LoadSceneMode.Additive);
            yield return null;
        }

        private async void OnSubmitCodeClicked()
        {
            string code = _codeText.text;
            code = code.Substring(0, code.Length - 1);

            bool succeeded = await GameLobbyManager.Instance.JoinLobby(code);
            if (succeeded)
            {
                StartCoroutine(LoadingAsync());
            }
        }
    }
}