using Snake;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace UI
{
    public class ResultPanel : MonoBehaviour
    {
        [Header("文本")]
        [SerializeField] private TMP_Text lengthText;
        [SerializeField] private TMP_Text resultText;
        
        [Header("结算背景")]
        [SerializeField] private Sprite success;
        [SerializeField] private Sprite failure;
        private Image background;
        
        [Header("星星")]
        [SerializeField] private Image[] starImages;

        [Header("按钮")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button menuButton;
        
        [SerializeField] private GameObject mask;
        private Core.GameManager gameManager;

        public void Init(Core.GameManager _gameManager)
        {
            gameManager = _gameManager;
            background = GetComponent<Image>();
            if (retryButton) retryButton.onClick.AddListener(() => gameManager?.RestartLevel());
            if (nextButton) nextButton.onClick.AddListener(() => gameManager?.LoadNextLevel());
            if (menuButton) menuButton.onClick.AddListener(() => gameManager?.ReturnToMenu());
        }

        public void ShowWin()
        {
            SnakeController snake = FindObjectOfType<SnakeController>();
            int len = snake != null ? snake.Length : 0;
            float occ = gameManager != null ? gameManager.CalculateOccupancy() : 0f;
            int stars = gameManager != null ? gameManager.CalculateStars(occ) : 0;

            if (lengthText) lengthText.text = $"猫长: {len}";
            if (resultText) resultText.text = $"占用率: {occ:P1}";
            mask.SetActive(true);
            ShowStars(stars);
            if (nextButton) nextButton.gameObject.SetActive(true);
            background.sprite = success;
            gameObject.SetActive(true);
        }

        public void ShowLose()
        {
            SnakeController snake = FindObjectOfType<SnakeController>();
            int len = snake != null ? snake.Length : 0;
            float occ = gameManager != null ? gameManager.CalculateOccupancy() : 0f;

            if (lengthText) lengthText.text = $"猫长: {len}";
            if (resultText) resultText.text = $"占用率: {occ:P1}";
            mask.SetActive(true);
            ShowStars(0);
            if (nextButton) nextButton.gameObject.SetActive(false);
            background.sprite = failure;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            mask.SetActive(false);
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
