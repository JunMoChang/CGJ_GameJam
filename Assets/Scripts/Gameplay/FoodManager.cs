using System.Collections.Generic;
using Snake;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class FoodManager : MonoBehaviour
    {
        [Header("预制体")]
        [SerializeField] private GameObject[] foodPrefabs;

        [Header("数量")]
        [SerializeField] private int richThreshold = 30;
        [SerializeField] private int minFoodWhenRich = 3;
        [SerializeField] private int maxFoodWhenRich = 5;

        private Grid.GridManager gridManager;
        private SnakeController snake;
        private readonly List<Vector2Int> foodPositions = new();
        private readonly Dictionary<Vector2Int, GameObject> foodVisuals = new();

        public void Init(Grid.GridManager gm, SnakeController snakeCtrl)
        {
            gridManager = gm;
            snake = snakeCtrl;
            ClearAll();
        }
        
        public void OnEatFood(Vector2Int pos)
        {
            foodPositions.Remove(pos);

            if (foodVisuals.TryGetValue(pos, out var go))
            {
                if (go != null) Destroy(go);
                foodVisuals.Remove(pos);
            }

            RefreshFoods();
        }
        
        public void RefreshFoods()
        {
            HashSet<Vector2Int> exclude = new HashSet<Vector2Int>();
            foreach (Vector2Int fp in foodPositions) exclude.Add(fp);
            
            foreach (Vector2Int bp in snake.BodyPositions)
            {
                exclude.Add(bp);
            }

            List<Vector2Int> spawnable = gridManager.GetSpawnableCells(exclude);
            int totalFree = spawnable.Count + foodPositions.Count;
            int target = CalculateTarget(totalFree);
            int need = target - foodPositions.Count;

            for (int i = 0; i < need && spawnable.Count > 0; i++)
            {
                int idx = Random.Range(0, spawnable.Count);
                Vector2Int pos = spawnable[idx];
                spawnable.RemoveAt(idx);

                foodPositions.Add(pos);
                gridManager.SetState(pos, Grid.GridCellState.Food);

                if (foodPrefabs is { Length: > 0 })
                {
                    GameObject prefab = foodPrefabs[Random.Range(0, foodPrefabs.Length)];
                    GameObject go = Instantiate(prefab, gridManager.GridToWorld(pos), Quaternion.identity);
                    foodVisuals[pos] = go;
                }
            }
        }

        private int CalculateTarget(int spawnableCount)
        {
            if (spawnableCount >= richThreshold) return Random.Range(minFoodWhenRich, maxFoodWhenRich + 1);
            return Mathf.Max(1, Mathf.FloorToInt(spawnableCount * 0.1f));
        }

        public void ClearAll()
        {
            foreach (GameObject v in foodVisuals.Values)
            {
                if (v != null) Destroy(v);
            }
            foodVisuals.Clear();
            foodPositions.Clear();
        }
    }
}
