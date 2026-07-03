using System.Collections.Generic;
using Snake;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    /// <summary>
    /// 成长道具管理：生成、补充、被吃响应
    /// </summary>
    public class FoodManager : MonoBehaviour
    {
        [Header("预制体")]
        public GameObject foodPrefab;

        [Header("数量")]
        [SerializeField] private int richThreshold = 30;
        [SerializeField] private int minFoodWhenRich = 3;
        [SerializeField] private int maxFoodWhenRich = 5;

        private Grid.GridManager gridManager;
        private readonly List<Vector2Int> foodPositions = new();
        private readonly List<GameObject> foodVisuals = new();

        public void Init(Grid.GridManager gm)
        {
            gridManager = gm;
            ClearAll();
        }

        /// <summary>蛇吃到食物时调用</summary>
        public void OnEatFood(Vector2Int pos)
        {
            // 移除食物
            for (int i = foodPositions.Count - 1; i >= 0; i--)
            {
                if (foodPositions[i] == pos)
                {
                    foodPositions.RemoveAt(i);
                    break;
                }
            }
            // 移除显示
            for (int i = foodVisuals.Count - 1; i >= 0; i--)
            {
                if (foodVisuals[i] != null &&
                    Vector2Int.RoundToInt(foodVisuals[i].transform.position) == pos)
                {
                    Destroy(foodVisuals[i]);
                    foodVisuals.RemoveAt(i);
                    break;
                }
            }

            // 补充
            RefreshFoods();
        }

        /// <summary>刷新食物到目标数量</summary>
        public void RefreshFoods()
        {
            var exclude = new HashSet<Vector2Int>();
            foreach (var fp in foodPositions) exclude.Add(fp);

            // 排除蛇身
            var snake = FindObjectOfType<SnakeController>();
            if (snake != null)
                foreach (var bp in snake.BodyPositions)
                    exclude.Add(bp);

            var spawnable = gridManager.GetSpawnableCells(exclude);
            int totalFree = spawnable.Count + foodPositions.Count;
            int target = CalculateTarget(totalFree);
            int need = target - foodPositions.Count;

            for (int i = 0; i < need && spawnable.Count > 0; i++)
            {
                int idx = Random.Range(0, spawnable.Count);
                var pos = spawnable[idx];
                spawnable.RemoveAt(idx);

                foodPositions.Add(pos);
                gridManager.SetState(pos, Grid.GridCellState.Food);

                if (foodPrefab != null)
                {
                    var go = Instantiate(foodPrefab, gridManager.GridToWorld(pos), Quaternion.identity);
                    foodVisuals.Add(go);
                }
            }
        }

        private int CalculateTarget(int spawnableCount)
        {
            if (spawnableCount >= richThreshold)
                return Random.Range(minFoodWhenRich, maxFoodWhenRich + 1);
            return Mathf.Max(1, Mathf.FloorToInt(spawnableCount * 0.1f));
        }

        private void ClearAll()
        {
            foreach (var v in foodVisuals)
                if (v != null) Destroy(v);
            foodVisuals.Clear();
            foodPositions.Clear();
        }
    }
}
