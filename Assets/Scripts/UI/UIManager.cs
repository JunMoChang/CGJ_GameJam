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
        private GameManager gm;

        private void Start()
        {
            gm = GameManager.Instance;
            mainMenu?.Init(gm);
            hud?.Init(gm);
            resultPanel?.Init(gm);

            gm.OnStateChanged += OnGameStateChanged;
            ShowStartScreen();
        }

        private void OnDestroy()
        {
            if (gm != null)
                gm.OnStateChanged -= OnGameStateChanged;
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
            if (hud)
            {
                hud.gameObject.SetActive(true);
                hud.RefreshAll();
            }
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
