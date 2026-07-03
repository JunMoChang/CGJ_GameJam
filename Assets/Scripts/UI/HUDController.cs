using Snake;
using UnityEngine;
using TMPro;

namespace UI
{
    /// <summary>
    /// HUD：关卡号、蛇长、占用率、目标、教学提示
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("文本")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text lengthText;
        [SerializeField] private TMP_Text occupancyText;

        [Header("教学")]
        [SerializeField] private GameObject attackTutorial;

        private Core.GameManager gm;
        private SnakeController snake;

        private void Start()
        {
            gm = Core.GameManager.Instance;
            snake = FindObjectOfType<SnakeController>();

            if (gm != null)
                gm.OnStateChanged += _ => RefreshAll();
        }

        private void Update()
        {
            if (gm == null || gm.CurrentState != Core.GameState.Playing) return;
            if (snake != null && lengthText != null)
                lengthText.text = $"蛇长: {snake.Length}";
            if (occupancyText != null && gm != null)
                occupancyText.text = $"占用率: {gm.CalculateOccupancy():P1}";
        }

        private void RefreshAll()
        {
            var cfg = gm?.CurrentLevelConfig;
            if (cfg == null) return;

            if (levelText != null)
                levelText.text = $"关卡 {cfg.levelIndex}";

            if (attackTutorial != null)
                attackTutorial.SetActive(cfg.enableAttackTutorial);
        }
    }
}
