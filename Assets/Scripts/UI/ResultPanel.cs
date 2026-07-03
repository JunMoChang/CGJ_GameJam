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

        private void Awake()
        {
            gm = Core.GameManager.Instance;

            if (gm != null)
                gm.OnStateChanged += OnStateChanged;

            if (retryButton) retryButton.onClick.AddListener(() => gm?.RestartLevel());
            if (nextButton) nextButton.onClick.AddListener(() => gm?.LoadNextLevel());
            if (menuButton) menuButton.onClick.AddListener(() => gm?.ReturnToMenu());

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (gm != null) gm.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(Core.GameState state)
        {
            if (state == Core.GameState.Win)
                Show(true);
            else if (state == Core.GameState.Lose)
                Show(false);
            else
                gameObject.SetActive(false);
        }

        private void Show(bool win)
        {
            var snake = FindObjectOfType<SnakeController>();
            int len = snake != null ? snake.Length : 0;
            float occ = gm != null ? gm.CalculateOccupancy() : 0f;
            int stars = win && gm != null ? gm.CalculateStars(occ) : 0;

            if (titleText) titleText.text = win ? "通关!" : "失败";
            if (resultText) resultText.text = $"蛇长: {len}  占用率: {occ:P1}";

            if (starImages != null)
            {
                for (int i = 0; i < starImages.Length; i++)
                {
                    if (starImages[i] != null) starImages[i].enabled = i < stars;
                }
            }

            if (nextButton) nextButton.gameObject.SetActive(win);

            gameObject.SetActive(true);
        }
    }
}
