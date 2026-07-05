using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("面板引用")]
        [SerializeField] private GameObject startScreen;
        [SerializeField] private MainMenuController mainMenu;
        [SerializeField] private HUDController hud;
        [SerializeField] private ResultPanel resultPanel;
        [SerializeField] private Button startButton;
        private GameManager gameManager;

        private void Start()
        {
            gameManager = GameManager.Instance;
            mainMenu?.Init(gameManager);
            hud?.Init(gameManager);
            resultPanel?.Init(gameManager);

            gameManager.OnStateChanged += OnGameStateChanged;
            ShowStartScreen();
        }

        private void OnDestroy()
        {
            if (gameManager != null) gameManager.OnStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.StartMenu:
                    ShowStartScreen();
                    break;
                case GameState.MainMenu:
                    ShowMainMenu();
                    break;
                case GameState.Playing:
                    ShowHUD();
                    break;
                case GameState.Win:
                case GameState.Lose:
                    ShowResult(state == GameState.Win);
                    break;
            }
        }

        private void ShowStartScreen()
        {
            startButton.gameObject.SetActive(true);
            if (startScreen) startScreen.SetActive(true);
            mainMenu?.gameObject.SetActive(false);
            if (hud) hud.gameObject.SetActive(false);
            resultPanel?.Hide();
        }

        private void ShowMainMenu()
        {
            if (startScreen) startScreen.SetActive(false);
            mainMenu?.gameObject.SetActive(true);
            if (hud) hud.gameObject.SetActive(false);
            resultPanel?.Hide();
        }

        private void ShowHUD()
        {
            mainMenu?.gameObject.SetActive(false);
            resultPanel?.Hide();
            
            hud.Show();
        }

        private void ShowResult(bool win)
        {
            if (win)
                resultPanel?.ShowWin();
            else
                resultPanel?.ShowLose();
        }
    }
}
