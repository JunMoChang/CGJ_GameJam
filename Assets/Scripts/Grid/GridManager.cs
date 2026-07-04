using System.Collections.Generic;
using UnityEngine;

namespace Grid
{
    public class GridManager : MonoBehaviour
    {
        private Vector2Int gridSize;
        private GridCellState[,] cells;
        private Dictionary<Vector2Int, GameObject> cellObjects;
        private HashSet<Vector2Int> emptyCells;

        [Header("显示")]
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private GameObject tilePrefab;

        private Vector2 gridOrigin;

        public int Width => gridSize.x;
        public int Height => gridSize.y;
        public float CellSize => cellSize;
        public Vector2 Origin => gridOrigin;

        public int EmptyCellCount { get; private set; }

        public void Init(Vector2Int size)
        {
            if (cellObjects != null)
            {
                foreach (GameObject go in cellObjects.Values)
                {
                    if (go != null)
                        Destroy(go);
                }
            }

            gridSize = size;
            cells = new GridCellState[size.x, size.y];
            cellObjects = new Dictionary<Vector2Int, GameObject>();
            EmptyCellCount = size.x * size.y;
            emptyCells = new HashSet<Vector2Int>(EmptyCellCount);
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    emptyCells.Add(new Vector2Int(x, y));
                }
            }
            
            gridOrigin.x = -(size.x * cellSize / 2f - cellSize / 2f);
            gridOrigin.y = -(size.y * cellSize / 2f - cellSize / 2f);

            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                {
                    cells[x, y] = GridCellState.Empty;

                    if (tilePrefab != null)
                    {
                        Vector2Int pos = new Vector2Int(x, y);
                        GameObject tile = Instantiate(tilePrefab, GridToWorld(pos), Quaternion.identity);
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

        public void SetState(Vector2Int pos, GridCellState currentState)
        {
            if (!IsInside(pos)) return;

            bool wasEmpty = cells[pos.x, pos.y] == GridCellState.Empty;
            bool isEmpty = currentState == GridCellState.Empty;

            cells[pos.x, pos.y] = currentState;

            if (wasEmpty && !isEmpty) { EmptyCellCount--; emptyCells.Remove(pos); }
            else if (!wasEmpty && isEmpty) { EmptyCellCount++; emptyCells.Add(pos); }
        }

        /// <summary>网格坐标 → 世界坐标（格子中心）</summary>
        public Vector3 GridToWorld(Vector2Int pos)
        {
            return new Vector3(
                gridOrigin.x + pos.x * cellSize,
                gridOrigin.y + pos.y * cellSize,
                0f);
        }

        /// <summary>蛇头初始位置</summary>
        public Vector2Int GetInitialHeadPosition()
        {
            return new Vector2Int(gridSize.x / 2, gridSize.y / 2 - 1);
        }
        
        /// <summary>获取所有空格且不在排除列表的格子</summary>
        public List<Vector2Int> GetSpawnableCells(HashSet<Vector2Int> excludePositions)
        {
            List<Vector2Int> result = new List<Vector2Int>(emptyCells.Count);
            foreach (Vector2Int pos in emptyCells)
            {
                if (!excludePositions.Contains(pos)) result.Add(pos);
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

        public void ClearTiles()
        {
            if (cellObjects != null)
            {
                foreach (var go in cellObjects.Values)
                    if (go != null) Destroy(go);
                cellObjects.Clear();
            }
        }
    }
}
