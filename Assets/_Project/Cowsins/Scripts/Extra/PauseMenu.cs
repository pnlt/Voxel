using UnityEngine;
using System;

namespace cowsins
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject playerUI;
        [SerializeField] private bool disablePlayerUIWhilePaused;
        [SerializeField] private CanvasGroup menu;
        [SerializeField] private float fadeSpeed;

        public static PauseMenu Instance { get; private set; }
        public static bool isPaused { get; private set; }

        [HideInInspector] public PlayerStats stats;

        public Action OnPause, OnUnpause;

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;

            isPaused = false;
            menu.gameObject.SetActive(false);
            menu.alpha = 0;
        }

        private void Update()
        {
            HandlePauseInput();

            if (isPaused)
            {
                HandlePause();
            }
            else
            {
                HandleUnpause();
            }
        }

        private void HandlePauseInput()
        {
            if (InputManager.pausing)
                isPaused = !isPaused;
        }

        private void HandlePause()
        {
            stats.LoseControl();

            if (!menu.gameObject.activeSelf)
            {
                menu.gameObject.SetActive(true);
                menu.alpha = 0;
            }

            if (menu.alpha < 1)
                menu.alpha += Time.deltaTime * fadeSpeed;

            if (disablePlayerUIWhilePaused && !stats.isDead)
                playerUI.SetActive(false);
        }

        private void HandleUnpause()
        {
            menu.alpha -= Time.deltaTime * fadeSpeed;

            if (menu.alpha <= 0)
                menu.gameObject.SetActive(false);

            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;

            playerUI.SetActive(true);
        }

        public void UnPause()
        {
            isPaused = false;
            stats.CheckIfCanGrantControl();
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
            playerUI.SetActive(true);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public void TogglePause()
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (disablePlayerUIWhilePaused && !stats.isDead)
                    playerUI.SetActive(false);
            }
            else
            {
                stats.CheckIfCanGrantControl();
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
                playerUI.SetActive(true);
            }
        }
    }
}
