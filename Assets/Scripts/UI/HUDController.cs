using Snake;
using UnityEngine;
using TMPro;

namespace UI
{
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

        public void Init(Core.GameManager gameManager)
        {
            gm = gameManager;
            snake = FindObjectOfType<SnakeController>();
        }

        public void RefreshAll()
        {
            var cfg = gm?.CurrentLevelConfig;
            if (cfg != null && levelText != null)
                levelText.text = $"关卡 {cfg.levelIndex}";
            if (attackTutorial != null && cfg != null)
                attackTutorial.SetActive(cfg.enableAttackTutorial);
        }

        private void Update()
        {
            if (gm == null || gm.CurrentState != Core.GameState.Playing) return;
            if (snake != null && lengthText != null)
                lengthText.text = $"蛇长: {snake.Length}";
            if (occupancyText != null)
                occupancyText.text = $"占用率: {gm.CalculateOccupancy():P1}";
        }
    }
}
