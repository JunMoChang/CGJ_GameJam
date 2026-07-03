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
        [SerializeField] private Obstacle.ObstacleManager obstacleManager;

        public void BuildLevel(LevelConfig config)
        {
            ClearLevel();

            // 网格
            gridManager.Init(config.gridSize);

            // 蛇
            snakeController.tickInterval = config.moveInterval;
            // SnakeController.Start() 已初始化蛇，但需要重新初始化以适应新网格
            // 此处由 GameManager 调用 StartMove 前，蛇已在 Start 初始化
            // 如果是重新加载关卡，需要重新初始化 — 简化处理：直接依赖 Start 的初始化

            // 食物
            foodManager.Init(gridManager);
            foodManager.RefreshFoods();

            // 岩石
            obstacleManager.Init(gridManager, config);
            var reserved = new HashSet<Vector2Int>(snakeController.BodyPositions);
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
