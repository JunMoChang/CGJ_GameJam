using Config;
using Snake;
using UnityEngine;

namespace UI
{
    public class TutorialPopupController : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private SnakeController snakeController;

        private bool pausedSnakeByTutorial;

        public void TryShowForLevel(LevelConfig config)
        {
            if (config == null) return;
            if (config.levelIndex != 3) return;
            if (!config.enableAttackTutorial) return;

            Show();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            if (snakeController == null)
            {
                snakeController = FindObjectOfType<SnakeController>();
            }

            if (snakeController != null)
            {
                snakeController.StopMove();
                pausedSnakeByTutorial = true;
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);

            if (pausedSnakeByTutorial && snakeController != null)
            {
                snakeController.StartMove();
            }

            pausedSnakeByTutorial = false;
        }
    }
}