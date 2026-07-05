using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("关卡按钮（1-5）")]
        [SerializeField] private Button[] levelButtons;

        private Core.GameManager gameManager;

        public void Init(Core.GameManager _gameManager)
        {
            gameManager = _gameManager;

            for (int i = 0; i < levelButtons.Length; i++)
            {
                int levelIndex = i + 1;
                if (levelButtons[i] != null)
                    levelButtons[i].onClick.AddListener(() => gameManager?.StartLevel(levelIndex));
            }
        }
    }
}
