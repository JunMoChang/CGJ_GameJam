using System.Collections.Generic;
using UnityEngine;

namespace Grid
{
    public class GridManager : MonoBehaviour
    {
        private Vector2Int gridSize;
        private GridCellState[,] cells;
        private Dictionary<Vector2Int, GameObject> cellObjects;

        [Header("显示")]
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private GameObject tilePrefab;

        private Vector2 gridOrigin;

        public int Width => gridSize.x;
        public int Height => gridSize.y;
        public float CellSize => cellSize;
        public Vector2 Origin => gridOrigin;

        public void Init(Vector2Int size)
        {
            // 清理旧视觉
            if (cellObjects != null)
                foreach (var go in cellObjects.Values)
                    if (go != null) Destroy(go);

            gridSize = size;
            cells = new GridCellState[size.x, size.y];
            cellObjects = new Dictionary<Vector2Int, GameObject>();

            // 自动居中：计算原点使网格中心在世界坐标 (0,0)
            gridOrigin.x = -(size.x * cellSize / 2f - cellSize / 2f);
            gridOrigin.y = -(size.y * cellSize / 2f - cellSize / 2f);

            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                {
                    cells[x, y] = GridCellState.Empty;

                    if (tilePrefab != null)
                    {
                        var pos = new Vector2Int(x, y);
                        var tile = Instantiate(tilePrefab, GridToWorld(pos), Quaternion.identity);
                        cellObjects[pos] = tile;
                    }
                }
        }

        public bool IsInside(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < gridSize.x && pos.y >= 0 && pos.y < gridSize.y;
        }

        public GridCellState GetState(Vector2Int pos)
        {
            if (!IsInside(pos)) return GridCellState.Empty;
            return cells[pos.x, pos.y];
        }

        public void SetState(Vector2Int pos, GridCellState state)
        {
            if (!IsInside(pos)) return;
            cells[pos.x, pos.y] = state;
        }

        public bool IsEmpty(Vector2Int pos)
        {
            return GetState(pos) == GridCellState.Empty;
        }

        /// <summary>网格坐标 → 世界坐标（格子中心）</summary>
        public Vector3 GridToWorld(Vector2Int pos)
        {
            return new Vector3(
                gridOrigin.x + pos.x * cellSize,
                gridOrigin.y + pos.y * cellSize,
                0f);
        }

        /// <summary>蛇头初始位置: 水平居中偏右、垂直居中</summary>
        public Vector2Int GetInitialHeadPosition()
        {
            return new Vector2Int(gridSize.x / 2, gridSize.y / 2 - 1);
        }

        public List<Vector2Int> GetEmptyCells()
        {
            var result = new List<Vector2Int>();
            for (int x = 0; x < gridSize.x; x++)
                for (int y = 0; y < gridSize.y; y++)
                    if (cells[x, y] == GridCellState.Empty)
                        result.Add(new Vector2Int(x, y));
            return result;
        }

        /// <summary>可生成的格子：Empty 但排除 Food 所在格</summary>
        public List<Vector2Int> GetSpawnableCells()
        {
            return GetEmptyCells(); // Food 也是 Empty 状态？不，Food 是独立状态
        }

        /// <summary>获取所有 Empty 且不是 Food 的格子</summary>
        public List<Vector2Int> GetSpawnableCells(HashSet<Vector2Int> excludePositions)
        {
            var result = new List<Vector2Int>();
            for (int x = 0; x < gridSize.x; x++)
                for (int y = 0; y < gridSize.y; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (cells[x, y] == GridCellState.Empty && !excludePositions.Contains(pos))
                        result.Add(pos);
                }
            return result;
        }

        /// <summary>蛇头 NxN 安全区内不得生成物品</summary>
        public bool IsInHeadSafeZone(Vector2Int pos, Vector2Int headPos, int safeZoneSize)
        {
            int half = safeZoneSize / 2;
            return pos.x >= headPos.x - half && pos.x <= headPos.x + half
                && pos.y >= headPos.y - half && pos.y <= headPos.y + half;
        }
    }
}
