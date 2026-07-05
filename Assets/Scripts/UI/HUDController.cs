using Config;
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
        [SerializeField] private TMP_Text targetText;

        [Header("教学")]
        [SerializeField] private GameObject attackTutorial;

        private Core.GameManager gm;
        private SnakeController snake;

        public void Init(Core.GameManager gameManager)
        {
            gm = gameManager;
            snake = FindObjectOfType<SnakeController>();
            targetText.text = "60%";
        }

        public void RefreshAll()
        {
            LevelConfig cfg = gm?.CurrentLevelConfig;
            if (cfg != null && levelText != null)
                levelText.text = $"第{cfg.level}关";
            if (attackTutorial != null && cfg != null)
                attackTutorial.SetActive(cfg.enableAttackTutorial);
        }

        private void Update()
        {
            if (gm == null || gm.CurrentState != Core.GameState.Playing) return;
            if (snake != null && lengthText != null)
                lengthText.text = $"{snake.Length}";
            if (occupancyText != null)
                occupancyText.text = $"{gm.CalculateOccupancy():P1}";
        }
    }
}
