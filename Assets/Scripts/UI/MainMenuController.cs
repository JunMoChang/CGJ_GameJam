using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 主菜单：开始、选关、退出
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("按钮")]
        [SerializeField] private Button[] levelButtons; // 5 个关卡按钮

        private Core.GameManager gm;

        private void Awake()
        {
            gm = Core.GameManager.Instance;

            for (int i = 0; i < levelButtons.Length; i++)
            {
                int idx = i + 1;
                if (levelButtons[i] != null)
                    levelButtons[i].onClick.AddListener(() => gm?.StartLevel(idx));
            }

            if (gm != null)
                gm.OnStateChanged += OnStateChanged;
        }

        private void OnDestroy()
        {
            if (gm != null) gm.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(Core.GameState state)
        {
            gameObject.SetActive(state == Core.GameState.MainMenu);
        }
    }
}
