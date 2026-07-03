using System;
using UnityEngine;
using Config;
using Snake;

namespace Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Win,
        Lose
    }
    
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("配置")]
        public LevelDatabase levelDatabase;

        [Header("目标")]
        [Range(0f, 1f)] public float winOccupancy = 0.6f;

        [Header("引用")]
        [SerializeField] private SnakeController snakeController;
        [SerializeField] private Grid.GridManager gridManager;
        [SerializeField] private Gameplay.LevelSpawner levelSpawner;
        [SerializeField] private Obstacle.ObstacleManager obstacleManager;

        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public int CurrentLevelIndex { get; private set; }
        public LevelConfig CurrentLevelConfig => levelDatabase?.GetLevel(CurrentLevelIndex);

        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance == null) 
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void OnEnable()
        {
            if (snakeController != null)
            {
                snakeController.OnMoved += OnSnakeMoved;
                snakeController.OnDead += OnSnakeDead;
            }
        }

        private void OnDisable()
        {
            if (snakeController != null)
            {
                snakeController.OnMoved -= OnSnakeMoved;
                snakeController.OnDead -= OnSnakeDead;
            }
        }
        
        public void StartLevel(int levelIndex)
        {
            LevelConfig cfg = levelDatabase?.GetLevel(levelIndex);
            if (cfg == null) return;

            CurrentLevelIndex = levelIndex;
            SetState(GameState.Playing);
            levelSpawner?.BuildLevel(cfg);
        }
        
        public void RestartLevel() => StartLevel(CurrentLevelIndex);

        public void LoadNextLevel()
        {
            int next = CurrentLevelIndex + 1;
            if (levelDatabase != null && next <= levelDatabase.LevelCount)
                StartLevel(next);
            else
                SetState(GameState.Win);
        }

        public void ReturnToMenu()
        {
            levelSpawner?.ClearLevel();
            SetState(GameState.MainMenu);
        }

        private void OnSnakeMoved(Vector2Int headPos)
        {
            float occ = CalculateOccupancy();
            if (occ >= winOccupancy)
                SetState(GameState.Win);

            // 第 5 关随机岩石
            obstacleManager?.TrySpawnRandomObstacle(headPos);
        }

        private void OnSnakeDead()
        {
            SetState(GameState.Lose);
        }

        public float CalculateOccupancy()
        {
            if (snakeController == null || gridManager == null) return 0f;
            int total = gridManager.Width * gridManager.Height;
            return total > 0 ? snakeController.Length / (float)total : 0f;
        }

        public int CalculateStars(float occupancy)
        {
            if (occupancy >= 0.7f) return 3;
            if (occupancy >= 0.5f) return 2;
            return 1;
        }

        private void SetState(GameState state)
        {
            if (CurrentState == state) return;
            CurrentState = state;

            if (state == GameState.Playing)
                snakeController?.StartMove();
            else if (state is GameState.Paused or GameState.Win or GameState.Lose)
                snakeController?.StopMove();

            OnStateChanged?.Invoke(state);
        }
    }
}
