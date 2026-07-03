using System;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;

namespace Snake
{
    public class SnakeController : MonoBehaviour
    {
        [Header("移动节奏")]
        public float tickInterval = 0.5f;

        [Header("预制体")]
        [SerializeField] private GameObject headPrefab;
        [SerializeField] private GameObject bodyPrefab;

        [Header("攻击")]
        [SerializeField] private int attackRange = 2;
        [SerializeField] private float attackCooldown = 0.35f;

        [Header("引用")]
        [SerializeField] private Grid.GridManager gridManager;
        [SerializeField] private ObstacleManager obstacleManager;
        
        public event Action<int> OnLengthChanged;// 蛇长变化
        public event Action OnDead;// 死亡
        public event Action<Vector2Int> OnMoved;// 每次移动后，传蛇头坐标
        public event Action<Vector2Int> OnFoodEaten;// 吃到食物，传食物坐标
        public event Action<Vector2Int> OnObstacleDestroyed;// 攻击破坏岩石
        
        
        private readonly LinkedList<Vector2Int> bodyList = new();
        private readonly HashSet<Vector2Int> bodySet = new();
        private GameObject headVisual;
        private readonly List<GameObject> bodyVisuals = new();

        private Vector2Int currentDirection = Vector2Int.right;
        private Vector2Int pendingDirection = Vector2Int.right;

        private float tickTimer;
        private float lastAttackTime;
        private bool isGameOver;
        
        public Vector2Int HeadPosition => bodyList.Count > 0 ? bodyList.First.Value : Vector2Int.zero;
        public IReadOnlyCollection<Vector2Int> BodyPositions => bodySet;
        public int Length => bodyList.Count;
        public bool IsRunning => !isGameOver;
        public Vector2Int CurrentDirection => currentDirection;

        private void OnEnable()
        {
            if (InputManager.Instance != null)
                InputManager.Instance.OnInputPressed += HandleInputPressed;
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
            TryAttack();
        }

        private void Update()
        {
            if (isGameOver || bodyList.Count == 0) return;

            ReadDirectionInput();

            tickTimer += Time.deltaTime;
            if (tickTimer >= tickInterval)
            {
                tickTimer -= tickInterval;
                Tick();
            }
        }
        
        public void StartMove()
        {
            isGameOver = false;
            tickTimer = 0f;
        }
        
        public void StopMove()
        {
            isGameOver = true;
        }

        #region 初始化

        public void Init(int length)
        {
            bodyList.Clear();
            bodySet.Clear();

            Vector2Int headStartPos = gridManager != null ? gridManager.GetInitialHeadPosition() : new Vector2Int(6, 6);

            for (int i = 0; i < length; i++)
            {
                if (headStartPos.x - (length - 1) < 0)
                {
                    Debug.LogError($"initialLength({length}) 超出网格左边界，headStartPos.x={headStartPos.x}");
                    length = headStartPos.x + 1;
                }
                
                Vector2Int pos = headStartPos - new Vector2Int(i, 0);
                
                bodyList.AddLast(pos);
                bodySet.Add(pos);
            }

            currentDirection = Vector2Int.right;
            pendingDirection = Vector2Int.right;
            
            
            if (gridManager != null)
            {
                bool first = true;
                foreach (var pos in bodyList)
                {
                    gridManager.SetState(pos, first ? Grid.GridCellState.SnakeHead : Grid.GridCellState.SnakeBody);
                    first = false;
                }
            }

            RenderBody();
            OnLengthChanged?.Invoke(bodyList.Count);
        }

        #endregion

        #region 方向输入

        private void ReadDirectionInput()
        {
            Vector2 raw = InputManager.Instance.GetVector2("Move");
            if (raw == Vector2.zero) return;
            
            Vector2Int candidate = Mathf.Abs(raw.x) >= Mathf.Abs(raw.y) ? raw.x > 0 ? Vector2Int.right : Vector2Int.left : raw.y > 0 ? Vector2Int.up : Vector2Int.down;
            
            if (candidate == -currentDirection) return;

            pendingDirection = candidate;
        }

        #endregion

        #region 核心 Tick

        private void Tick()
        {
            currentDirection = pendingDirection;

            Vector2Int newHead = bodyList.First.Value + currentDirection;

            // 撞墙
            if (gridManager == null || !gridManager.IsInside(newHead))
            {
                Die();
                return;
            }

            // 撞岩石
            if (gridManager.GetState(newHead) == Grid.GridCellState.Obstacle)
            {
                Die();
                return;
            }

            // 撞自己
            Vector2Int tailPos = bodyList.Last.Value;
            bool ateFood = gridManager.GetState(newHead) == Grid.GridCellState.Food;

            if (bodySet.Contains(newHead) && !(!ateFood && newHead == tailPos))
            {
                Die();
                return;
            }

            // 更新蛇头
            if (gridManager != null)
                gridManager.SetState(bodyList.First.Value, Grid.GridCellState.SnakeBody);

            bodyList.AddFirst(newHead);
            bodySet.Add(newHead);

            if (gridManager != null)
                gridManager.SetState(newHead, Grid.GridCellState.SnakeHead);

            // 移除尾部或增长
            if (!ateFood)
            {
                Vector2Int removed = bodyList.Last.Value;
                bodyList.RemoveLast();
                bodySet.Remove(removed);
                if (gridManager != null)
                    gridManager.SetState(removed, Grid.GridCellState.Empty);
            }
            else
            {
                OnFoodEaten?.Invoke(newHead);
                OnLengthChanged?.Invoke(bodyList.Count);
            }

            // 刷新显示 & 事件
            RenderBody();
            OnMoved?.Invoke(newHead);
        }

        #endregion

        #region 攻击

        private void TryAttack()
        {
            if (bodyList.Count == 0) return;
            if (obstacleManager == null) return;
            if (Time.time - lastAttackTime < attackCooldown) return;

            lastAttackTime = Time.time;
            Vector2Int head = bodyList.First.Value;

            for (int i = 1; i <= attackRange; i++)
            {
                Vector2Int target = head + currentDirection * i;
                if (obstacleManager.DestroyObstacle(target))
                {
                    Debug.Log($"Destroyed obstacle at {target}");
                    OnObstacleDestroyed?.Invoke(target);
                    return;
                }
            }
        }

        #endregion

        #region 碰撞

        private void Die()
        {
            isGameOver = true;
            OnDead?.Invoke();
        }

        #endregion

        #region 显示

        private void RenderBody()
        {
            if (gridManager == null) return;

            // 蛇头（索引 0）
            if (headVisual == null && headPrefab != null)
                headVisual = Instantiate(headPrefab);

            if (headVisual != null)
            {
                headVisual.transform.position = gridManager.GridToWorld(bodyList.First.Value);
                headVisual.transform.rotation = Quaternion.Euler(0, 0, DirectionToAngle(currentDirection));
            }

            // 蛇身（索引 1..n-1）
            int bodyCount = bodyList.Count - 1;
            while (bodyVisuals.Count < bodyCount)
                bodyVisuals.Add(bodyPrefab != null ? Instantiate(bodyPrefab) : null);
            while (bodyVisuals.Count > bodyCount)
            {
                int last = bodyVisuals.Count - 1;
                if (bodyVisuals[last] != null) Destroy(bodyVisuals[last]);
                bodyVisuals.RemoveAt(last);
            }

            int i = 0;
            bool skipFirst = true;
            foreach (Vector2Int segment in bodyList)
            {
                if (skipFirst) { skipFirst = false; continue; }
                if (bodyVisuals[i] != null)
                    bodyVisuals[i].transform.position = gridManager.GridToWorld(segment);
                i++;
            }
        }

        private float DirectionToAngle(Vector2Int dir)
        {
            if (dir == Vector2Int.up)    return 0f;
            if (dir == Vector2Int.down)  return 180f;
            if (dir == Vector2Int.left)  return 90f;
            if (dir == Vector2Int.right) return -90f;
            return 0f;
        }

        #endregion
    }
}
