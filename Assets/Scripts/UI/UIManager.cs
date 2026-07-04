using Core;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// UI 总管：统一获取 GameManager 并注入给各个 UI 组件，保证时序
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("面板引用")]
        [SerializeField] private MainMenuController mainMenu;
        [SerializeField] private HUDController hud;
        [SerializeField] private ResultPanel resultPanel;

        private GameManager gm;

        private void Start()
        {
            gm = GameManager.Instance;
            mainMenu?.Init(gm);
            hud?.Init(gm);
            resultPanel?.Init(gm);

            gm.OnStateChanged += OnGameStateChanged;
            ShowMainMenu();
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

        private void ShowMainMenu()
        {
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
            if (hud) hud.gameObject.SetActive(false);
            if (win)
                resultPanel?.ShowWin();
            else
                resultPanel?.ShowLose();
        }
    }
}
