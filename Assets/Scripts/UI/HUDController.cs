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

        private Core.GameManager gameManager;
        private SnakeController snake;

        public void Init(Core.GameManager _gameManager)
        {
            gameManager = _gameManager;
            targetText.text = "60%";
            snake = FindObjectOfType<SnakeController>();
            if (snake != null) snake.OnLengthChanged += RefreshLength;
        }

        private void OnDestroy()
        {
            if (snake != null)
                snake.OnLengthChanged -= RefreshLength;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            InitializeRefresh();
            RefreshLength(snake != null ? snake.Length : 0);
        }

        private void RefreshLength(int length)
        {
            lengthText.text = $"{length}";
            occupancyText.text = $"{gameManager.CalculateOccupancy():P1}";
        }

        private void InitializeRefresh()
        {
            LevelConfig cfg = gameManager?.CurrentLevelConfig;
            if (cfg != null && levelText != null)
                levelText.text = $"第{cfg.level}关";
            if (attackTutorial != null && cfg != null)
                attackTutorial.SetActive(cfg.enableAttackTutorial);
        }
    }
}
