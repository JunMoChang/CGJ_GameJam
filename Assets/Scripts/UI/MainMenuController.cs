using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("关卡按钮（1-5）")]
        [SerializeField] private Button[] levelButtons;

        private Core.GameManager gm;

        public void Init(Core.GameManager gameManager)
        {
            gm = gameManager;

            for (int i = 0; i < levelButtons.Length; i++)
            {
                int levelIndex = i + 1;
                if (levelButtons[i] != null)
                    levelButtons[i].onClick.AddListener(() => gm?.StartLevel(levelIndex));
            }
        }
    }
}
