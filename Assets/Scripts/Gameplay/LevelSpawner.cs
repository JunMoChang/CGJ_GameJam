using System.Collections.Generic;
using UnityEngine;
using Config;
using Snake;

namespace Gameplay
{
    public class LevelSpawner : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Grid.GridManager gridManager;
        [SerializeField] private SnakeController snakeController;
        [SerializeField] private FoodManager foodManager;
        [SerializeField] private ObstacleManager obstacleManager;
        [SerializeField] private CameraFitter cameraFitter;

        public void BuildLevel(LevelConfig config)
        {
            ClearLevel();

            // 网格
            gridManager.Init(config.gridSize);
            cameraFitter?.FitToGrid(config.gridSize, gridManager.CellSize);

            // 蛇
            snakeController.tickInterval = config.moveInterval;
            snakeController.Init(config.initialSnakeLength);

            // 食物
            foodManager.Init(gridManager, snakeController);
            foodManager.RefreshFoods();

            // 岩石
            obstacleManager.Init(gridManager, config);
            HashSet<Vector2Int> reserved = new HashSet<Vector2Int>(snakeController.BodyPositions);
            obstacleManager.SpawnInitialObstacles(reserved, snakeController.HeadPosition, snakeController.CurrentDirection);
            
            snakeController.OnFoodEaten -= OnFoodEaten;
            snakeController.OnFoodEaten += OnFoodEaten;

            // 开始
            snakeController.StartMove();
        }

        public void ClearLevel()
        {
            if (snakeController != null) 
            {
                snakeController.StopMove();
                snakeController.ClearVisuals();
                snakeController.OnFoodEaten -= OnFoodEaten;
            }
            foodManager?.ClearAll();
            obstacleManager?.ClearAll();
            gridManager?.ClearTiles();
        }

        private void OnFoodEaten(Vector2Int pos) => foodManager.OnEatFood(pos);
    }
}
