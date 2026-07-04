using Snake;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace UI
{
    public class ResultPanel : MonoBehaviour
    {
        [Header("文本")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text resultText;

        [Header("星星")]
        [SerializeField] private Image[] starImages;

        [Header("按钮")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button menuButton;

        private Core.GameManager gm;

        public void Init(Core.GameManager gameManager)
        {
            gm = gameManager;

            if (retryButton) retryButton.onClick.AddListener(() => gm?.RestartLevel());
            if (nextButton) nextButton.onClick.AddListener(() => gm?.LoadNextLevel());
            if (menuButton) menuButton.onClick.AddListener(() => gm?.ReturnToMenu());
        }

        public void ShowWin()
        {
            var snake = FindObjectOfType<SnakeController>();
            int len = snake != null ? snake.Length : 0;
            float occ = gm != null ? gm.CalculateOccupancy() : 0f;
            int stars = gm != null ? gm.CalculateStars(occ) : 0;

            if (titleText) titleText.text = "通关!";
            if (resultText) resultText.text = $"蛇长: {len}  占用率: {occ:P1}";
            ShowStars(stars);
            if (nextButton) nextButton.gameObject.SetActive(true);
            gameObject.SetActive(true);
        }

        public void ShowLose()
        {
            var snake = FindObjectOfType<SnakeController>();
            int len = snake != null ? snake.Length : 0;
            float occ = gm != null ? gm.CalculateOccupancy() : 0f;

            if (titleText) titleText.text = "失败";
            if (resultText) resultText.text = $"蛇长: {len}  占用率: {occ:P1}";
            ShowStars(0);
            if (nextButton) nextButton.gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void ShowStars(int count)
        {
            if (starImages == null) return;
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null) starImages[i].enabled = i < count;
            }
        }
    }
}
