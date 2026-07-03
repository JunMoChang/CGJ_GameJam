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

        public void BuildLevel(LevelConfig config)
        {
            ClearLevel();

            // 网格
            gridManager.Init(config.gridSize);

            // 蛇
            snakeController.tickInterval = config.moveInterval;
            snakeController.Init(config.initialSnakeLength);

            // 食物
            foodManager.Init(gridManager);
            foodManager.RefreshFoods();

            // 岩石
            obstacleManager.Init(gridManager, config);
            HashSet<Vector2Int> reserved = new HashSet<Vector2Int>(snakeController.BodyPositions);
            obstacleManager.SpawnInitialObstacles(reserved);

            // 订阅食物被吃
            snakeController.OnFoodEaten += pos => foodManager.OnEatFood(pos);

            // 开始
            snakeController.StartMove();
        }

        public void ClearLevel()
        {
            snakeController?.StopMove();
            obstacleManager?.ClearAll();
        }
    }
}
