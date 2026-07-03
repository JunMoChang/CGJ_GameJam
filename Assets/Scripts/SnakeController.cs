using System;
using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [Header("网格设置")]
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 20;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector2 gridOrigin = Vector2.zero; // 世界坐标里网格 (0,0) 对应的位置

    [Header("移动节奏")]
    [SerializeField] private float tickInterval = 0.2f;

    [Header("初始状态")]
    [SerializeField] private int initialLength = 3;
        
    [Header("预制体")]
    [SerializeField] private Transform bodyPrefab;
    [SerializeField] private Transform foodPrefab;
    [SerializeField] private Transform obstaclePrefab;

    [Header("事件")]
    public Action<int> OnScoreChanged; // 当前蛇长度
    public Action OnGameOver;
    public Action<Vector2Int> OnObstacleDestroyed;
    
    [SerializeField] private int obstacleCount = 8;
    [SerializeField] private int destroyRange = 2;
    
    private readonly LinkedList<Vector2Int> body = new();
    private readonly HashSet<Vector2Int> bodySet = new();
    private readonly List<Transform> segmentVisuals = new();
    private readonly Dictionary<Vector2Int, Transform> obstacles = new();

    private Vector2Int currentDirection = Vector2Int.right;
    private Vector2Int pendingDirection = Vector2Int.right;

    private Vector2Int foodPosition;
    private Transform foodVisual;

    private float tickTimer;
    private bool isGameOver;

    private void Start()
    {
        InitializeSnake();
        SpawnFood();
        SpawnObstacles(obstacleCount);
    }
    
    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            Debug.Log("ajf");
            InputManager.Instance.OnInputPressed += HandleInputPressed;
        }
    }
 
    private void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnInputPressed -= HandleInputPressed;
    }
    
    private void HandleInputPressed(string actionName)
    {
        if (isGameOver) return;
        if (actionName != "Attack") return;
 
        TryDestroyObstacleAhead();
    }
    
    private void Update()
    {
        if (isGameOver) return;

        ReadDirectionInput();

        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            Tick();
        }
    }
        
    private void InitializeSnake()
    {
        body.Clear();
        bodySet.Clear();

        Vector2Int startHead = new Vector2Int(gridWidth / 2, gridHeight / 2);
            
        for (int i = 0; i < initialLength; i++)
        {
            Vector2Int pos = startHead - new Vector2Int(i, 0);
            body.AddLast(pos);
            bodySet.Add(pos);
        }

        currentDirection = Vector2Int.right;
        pendingDirection = Vector2Int.right;

        RenderBody();
        OnScoreChanged?.Invoke(body.Count);
    }

    private void ReadDirectionInput()
    {
        Vector2 raw = InputManager.Instance.GetVector2("Move");
        if (raw == Vector2.zero) return;

        Vector2Int candidate = Mathf.Abs(raw.x) > Mathf.Abs(raw.y)
            ? raw.x > 0 ? Vector2Int.right : Vector2Int.left
            : raw.y > 0 ? Vector2Int.up : Vector2Int.down;
        
        if (candidate == -currentDirection) return;

        pendingDirection = candidate;
    }
        

    private void Tick()
    {
        currentDirection = pendingDirection;

        Vector2Int newHead = body.First.Value + currentDirection;
        
        if (IsOutOfBounds(newHead))
        {
            HandleGameOver();
            return;
        }

        // 允许移动进"当前尾部即将空出的格子"——这里的 self-collision 检查要排除尾部，
        // 因为尾部这一帧如果不吃食物会被移除，不算真碰撞
        bool willRemoveTail = newHead != foodPosition;
        Vector2Int tailPos = body.Last.Value;

        if (IsSelfCollision(newHead) && !(willRemoveTail && newHead == tailPos))
        {
            HandleGameOver();
            return;
        }

        bool ateFood = newHead == foodPosition;

        body.AddFirst(newHead);
        bodySet.Add(newHead);

        if (!ateFood)
        {
            Vector2Int removed = body.Last.Value;
            body.RemoveLast();
            bodySet.Remove(removed);
        }
        else
        {
            SpawnFood();
            OnScoreChanged?.Invoke(body.Count);
        }

        RenderBody();
    }

    private bool IsOutOfBounds(Vector2Int pos)
    {
        return pos.x < 0 || pos.x >= gridWidth || pos.y < 0 || pos.y >= gridHeight;
    }
    
    private void TryDestroyObstacleAhead()
    {
        Debug.Log("TryDestroyObstacleAhead");
        Vector2Int headPos = body.First.Value;
 
        for (int step = 1; step <= destroyRange; step++)
        {
            Vector2Int checkPos = headPos + currentDirection * step;
 
            if (obstacles.TryGetValue(checkPos, out Transform visual))
            {
                if (visual != null) Destroy(visual.gameObject);
                obstacles.Remove(checkPos);
                OnObstacleDestroyed?.Invoke(checkPos);
                return; // 一次只摧毁最近的一个，不继续往远处找
            }
        }
    }
    
    private bool IsSelfCollision(Vector2Int pos)
    {
        return bodySet.Contains(pos);
    }

    private void HandleGameOver()
    {
        isGameOver = true;
        OnGameOver?.Invoke();
    }
    
    private void SpawnFood()
    {
        Vector2Int pos;
        int safety = 0;
        do
        {
            pos = new Vector2Int(UnityEngine.Random.Range(0, gridWidth), UnityEngine.Random.Range(0, gridHeight));
            safety++;
            
            if (safety > gridWidth * gridHeight) return;
        }
        while (bodySet.Contains(pos));

        foodPosition = pos;

        if (foodVisual == null && foodPrefab != null)
            foodVisual = Instantiate(foodPrefab);

        if (foodVisual != null)
            foodVisual.position = GridToWorld(foodPosition);
    }
        
    private void SpawnObstacles(int count)
    {
        for (int n = 0; n < count; n++)
        {
            Vector2Int pos;
            int safety = 0;
            do
            {
                pos = new Vector2Int(
                    UnityEngine.Random.Range(0, gridWidth),
                    UnityEngine.Random.Range(0, gridHeight));
                safety++;
                if (safety > gridWidth * gridHeight) return;
            }
            while (bodySet.Contains(pos) || pos == foodPosition || obstacles.ContainsKey(pos));
 
            Transform visual = obstaclePrefab != null ? Instantiate(obstaclePrefab) : null;
            if (visual != null) visual.position = GridToWorld(pos);
 
            obstacles[pos] = visual;
        }
    }
    
    private void RenderBody()
    {
        while (segmentVisuals.Count < body.Count)
        {
            segmentVisuals.Add(bodyPrefab != null ? Instantiate(bodyPrefab) : null);
        }
        while (segmentVisuals.Count > body.Count)
        {
            int lastIndex = segmentVisuals.Count - 1;
            if (segmentVisuals[lastIndex] != null) Destroy(segmentVisuals[lastIndex].gameObject);
            segmentVisuals.RemoveAt(lastIndex);
        }

        int i = 0;
        foreach (Vector2Int segment in body)
        {
            if (segmentVisuals[i] != null)
                segmentVisuals[i].position = GridToWorld(segment);
            i++;
        }
    }

    private Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            gridOrigin.x + gridPos.x * cellSize,
            gridOrigin.y + gridPos.y * cellSize,
            0f);
    }
        

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Vector3 size = new Vector3(gridWidth * cellSize, gridHeight * cellSize, 0f);
        Vector3 center = new Vector3(gridOrigin.x + size.x / 2f - cellSize / 2f,
            gridOrigin.y + size.y / 2f - cellSize / 2f, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}