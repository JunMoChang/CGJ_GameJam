using System.Collections.Generic;
using UnityEngine;

namespace Grid
{
    public class GridManager : MonoBehaviour
    {
        private Vector2Int gridSize;
        private GridCellState[,] cells;
        private Dictionary<Vector2Int, GameObject> cellObjects;

        [Header("棋盘显示")]
        [SerializeField] private float cellSize = 1f;

        [Header("棋盘格 Prefab")]
        [SerializeField] private GameObject tilePrefabA;
        [SerializeField] private GameObject tilePrefabB;

        private Vector2 gridOrigin;

        public int Width => gridSize.x;
        public int Height => gridSize.y;
        public float CellSize => cellSize;
        public Vector2 Origin => gridOrigin;
        public int EmptyCellCount { get; private set; }

        public void Init(Vector2Int size)
        {
            ClearTiles();

            gridSize = size;
            cells = new GridCellState[size.x, size.y];
            cellObjects = new Dictionary<Vector2Int, GameObject>();
            EmptyCellCount = size.x * size.y;

            gridOrigin.x = -(size.x * cellSize / 2f - cellSize / 2f);
            gridOrigin.y = -(size.y * cellSize / 2f - cellSize / 2f);

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    cells[x, y] = GridCellState.Empty;

                    Vector2Int pos = new Vector2Int(x, y);
                    GameObject prefab = GetTilePrefab(x, y);

                    if (prefab == null)
                        continue;

                    GameObject tile = Instantiate(prefab, GridToWorld(pos), Quaternion.identity, transform);
                    tile.name = $"GridCell_{x}_{y}";
                    tile.transform.localScale = new Vector3(cellSize, cellSize, 1f);

                    cellObjects[pos] = tile;
                }
            }
        }

        private GameObject GetTilePrefab(int x, int y)
        {
            if (tilePrefabA == null && tilePrefabB == null)
                return null;

            if (tilePrefabA == null)
                return tilePrefabB;

            if (tilePrefabB == null)
                return tilePrefabA;

            return (x + y) % 2 == 0 ? tilePrefabA : tilePrefabB;
        }

        public bool IsInside(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < gridSize.x && pos.y >= 0 && pos.y < gridSize.y;
        }

        public GridCellState GetState(Vector2Int pos)
        {
            if (!IsInside(pos))
                return GridCellState.Empty;

            return cells[pos.x, pos.y];
        }

        public void SetState(Vector2Int pos, GridCellState currentState)
        {
            if (!IsInside(pos))
                return;

            bool wasEmpty = cells[pos.x, pos.y] == GridCellState.Empty;
            bool isEmpty = currentState == GridCellState.Empty;

            cells[pos.x, pos.y] = currentState;

            if (wasEmpty && !isEmpty)
                EmptyCellCount--;
            else if (!wasEmpty && isEmpty)
                EmptyCellCount++;
        }

        public bool IsEmpty(Vector2Int pos)
        {
            return GetState(pos) == GridCellState.Empty;
        }

        public Vector3 GridToWorld(Vector2Int pos)
        {
            return new Vector3(
                gridOrigin.x + pos.x * cellSize,
                gridOrigin.y + pos.y * cellSize,
                0f
            );
        }

        public Vector2Int GetInitialHeadPosition()
        {
            return new Vector2Int(gridSize.x / 2, gridSize.y / 2 - 1);
        }

        public List<Vector2Int> GetEmptyCells()
        {
            List<Vector2Int> result = new();

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    if (cells[x, y] == GridCellState.Empty)
                        result.Add(new Vector2Int(x, y));
                }
            }

            return result;
        }

        public List<Vector2Int> GetSpawnableCells()
        {
            return GetEmptyCells();
        }

        public List<Vector2Int> GetSpawnableCells(HashSet<Vector2Int> excludePositions)
        {
            List<Vector2Int> result = new();

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    if (cells[x, y] == GridCellState.Empty && !excludePositions.Contains(pos))
                        result.Add(pos);
                }
            }

            return result;
        }

        public bool IsInHeadSafeZone(Vector2Int pos, Vector2Int headPos, int safeZoneSize)
        {
            int half = safeZoneSize / 2;

            return pos.x >= headPos.x - half
                && pos.x <= headPos.x + half
                && pos.y >= headPos.y - half
                && pos.y <= headPos.y + half;
        }

        public void ClearTiles()
        {
            if (cellObjects == null)
                return;

            foreach (GameObject go in cellObjects.Values)
            {
                if (go != null)
                    Destroy(go);
            }

            cellObjects.Clear();
        }
    }
}
