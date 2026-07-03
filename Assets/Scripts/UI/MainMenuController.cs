using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("关卡按钮（1-5）")]
        [SerializeField] private Button[] levelButtons;

        [Header("菜单根节点")]
        [SerializeField] private GameObject menuRoot;

        private Core.GameManager gm;

        private void Start()
        {
            gm = Core.GameManager.Instance;

            for (int i = 0; i < levelButtons.Length; i++)
            {
                int levelIndex = i + 1;
                if (levelButtons[i] != null)
                    levelButtons[i].onClick.AddListener(() => OnClickLevel(levelIndex));
            }
        }

        private void OnClickLevel(int levelIndex)
        {
            gm?.StartLevel(levelIndex);
            
            if (menuRoot != null) menuRoot.SetActive(false);
        }
    }
}
