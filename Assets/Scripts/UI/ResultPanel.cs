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
        
        [Header("星星")]
        [SerializeField] private Image[] starImages;

        [Header("按钮")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button menuButton;
        
        [SerializeField] private GameObject mask;
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

            if (lengthText) lengthText.text = $"{len}";
            if (resultText) resultText.text = $"{occ:P1}";
            mask.SetActive(true);
            ShowStars(stars);
            if (nextButton) nextButton.gameObject.SetActive(true);
            gameObject.SetActive(true);
        }

        public void ShowLose()
        {
            var snake = FindObjectOfType<SnakeController>();
            int len = snake != null ? snake.Length : 0;
            float occ = gm != null ? gm.CalculateOccupancy() : 0f;

            if (lengthText) lengthText.text = $"{len}";
            if (resultText) resultText.text = $"{occ:P1}";
            mask.SetActive(true);
            ShowStars(0);
            if (nextButton) nextButton.gameObject.SetActive(false);
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
