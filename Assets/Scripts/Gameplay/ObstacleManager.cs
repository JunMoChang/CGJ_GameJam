using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class ObstacleManager : MonoBehaviour
    {
        [Header("预制体")]
        public GameObject obstaclePrefab;

        private Grid.GridManager gridManager;
        private Config.LevelConfig config;
        private readonly HashSet<Vector2Int> obstaclePositions = new();
        private readonly Dictionary<Vector2Int, GameObject> obstacleVisuals = new();

        public void Init(Grid.GridManager gm, Config.LevelConfig cfg)
        {
            gridManager = gm;
            config = cfg;
        }

        public void ClearAll()
        {
            foreach (GameObject v in obstacleVisuals.Values)
            {
                if (v != null) Destroy(v);
            }
            obstacleVisuals.Clear();
            obstaclePositions.Clear();
        }

        /// <summary>生成初始岩石</summary>
        public void SpawnInitialObstacles(IEnumerable<Vector2Int> reservedCells, Vector2Int snakeHead, Vector2Int snakeDir)
        {
            if (config == null) return;

            HashSet<Vector2Int> reserved = new HashSet<Vector2Int>(reservedCells);

            // 教程关：在蛇头正前方 2 格生成一个障碍物
            if (config.enableAttackTutorial)
            {
                Vector2Int tutorialPos = snakeHead + snakeDir * 2;
                if (gridManager.IsInside(tutorialPos))
                {
                    PlaceObstacle(tutorialPos);
                    reserved.Add(tutorialPos);
                }
            }
            List<Vector2Int> candidates = new List<Vector2Int>();

            for (int x = 0; x < gridManager.Width; x++)
            {
                for (int y = 0; y < gridManager.Height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (gridManager.GetState(pos) == Grid.GridCellState.Empty && !reserved.Contains(pos)) candidates.Add(pos);
                }
            }

            int count = Random.Range(config.minInitialObstacles, config.maxInitialObstacles + 1);
            if (config.enableAttackTutorial) count = Mathf.Max(0, count - 1);

            for (int i = 0; i < count && candidates.Count > 0; i++)
            {
                List<Vector2Int> valid = candidates.Where(p => !IsAdjacentToObstacle(p)).ToList();
                if (valid.Count == 0) break;

                int idx = Random.Range(0, valid.Count);
                PlaceObstacle(valid[idx]);
                candidates.Remove(valid[idx]);
            }
        }

        /// <summary>第 5 关：随机生成岩石（避开蛇头安全区）</summary>
        public void TrySpawnRandomObstacle(Vector2Int snakeHeadPos)
        {
            if (config == null || !config.enableRandomObstacleSpawn) return;
            if (Random.value > config.randomObstacleChance) return;
            
            int maxObstacles = Mathf.Max(0, Mathf.FloorToInt(gridManager.EmptyCellCount * config.maxObstacleProportionOfEmptyGrid));
            if (obstaclePositions.Count >= maxObstacles) return;

            var candidates = new List<Vector2Int>();
            for (int x = 0; x < gridManager.Width; x++)
                for (int y = 0; y < gridManager.Height; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (gridManager.GetState(pos) != Grid.GridCellState.Empty) continue;
                    if (gridManager.IsInHeadSafeZone(pos, snakeHeadPos, config.safeZoneSize)) continue;
                    candidates.Add(pos);
                }

            if (candidates.Count == 0) return;
            PlaceObstacle(candidates[Random.Range(0, candidates.Count)]);
        }

        /// <summary>销毁指定位置的岩石，返回是否成功</summary>
        public bool DestroyObstacle(Vector2Int pos)
        {
            if (!obstaclePositions.Remove(pos)) return false;

            gridManager.SetState(pos, Grid.GridCellState.Empty);

            if (obstacleVisuals.TryGetValue(pos, out var go))
            {
                if (go != null) Destroy(go);
                obstacleVisuals.Remove(pos);
            }
            return true;
        }

        public bool IsAdjacentToObstacle(Vector2Int pos)
        {
            Vector2Int[] dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (Vector2Int d in dirs)
            {
                if (obstaclePositions.Contains(pos + d)) return true;
            }
            return false;
        }

        private void PlaceObstacle(Vector2Int pos)
        {
            obstaclePositions.Add(pos);
            gridManager.SetState(pos, Grid.GridCellState.Obstacle);

            if (obstaclePrefab != null)
            {
                GameObject go = Instantiate(obstaclePrefab, gridManager.GridToWorld(pos), Quaternion.identity);
                obstacleVisuals[pos] = go;
            }
        }
    }
}
