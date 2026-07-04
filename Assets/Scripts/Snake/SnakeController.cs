using System;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Snake
{
    public class SnakeController : MonoBehaviour
    {
        [Header("移动节奏")]
        public float tickInterval = 0.5f;

        [Header("预制体")]
        [SerializeField] private GameObject headPrefab;
        [SerializeField] private GameObject bodyPrefab;
        [SerializeField] private Sprite bodyImage;
        [SerializeField] private Sprite trailImage;
        [SerializeField] private Sprite turnPointImage;

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
        private readonly LinkedList<GameObject> bodyVisuals = new(); // 与 bodyList 去掉表头后的部分严格一一对应

        private Vector2Int currentDirection = Vector2Int.right;
        private Vector2Int pendingDirection = Vector2Int.right;
        private Vector2Int previousDirection = Vector2Int.right; // 上一次 Tick 使用的方向，用于计算新生成那一节的转弯形状

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
            previousDirection = Vector2Int.right;
            
            if (gridManager != null)
            {
                bool first = true;
                foreach (Vector2Int pos in bodyList)
                {
                    gridManager.SetState(pos, first ? Grid.GridCellState.SnakeHead : Grid.GridCellState.SnakeBody);
                    first = false;
                }
            }

            RenderBodyFull();
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
            previousDirection = currentDirection;
            currentDirection = pendingDirection;

            Vector2Int oldHeadPos = bodyList.First.Value;
            Vector2Int newHead = oldHeadPos + currentDirection;

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
            gridManager.SetState(oldHeadPos, Grid.GridCellState.SnakeBody);

            bodyList.AddFirst(newHead);
            bodySet.Add(newHead);

            gridManager.SetState(newHead, Grid.GridCellState.SnakeHead);

            // 移除尾部或增长
            if (!ateFood)
            {
                Vector2Int removed = bodyList.Last.Value;
                bodyList.RemoveLast();
                bodySet.Remove(removed);
                gridManager.SetState(removed, Grid.GridCellState.Empty);

                AdvanceVisuals(oldHeadPos);
            }
            else
            {
                OnFoodEaten?.Invoke(newHead);
                OnLengthChanged?.Invoke(bodyList.Count);

                GrowVisuals(oldHeadPos);
            }

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

        // 头部视觉：每帧都需要重新定位定向，本身没有身份延续问题
        private void UpdateHeadVisual()
        {
            if (gridManager == null) return;
            if (headVisual == null) headVisual = Instantiate(headPrefab);

            headVisual.transform.position = gridManager.GridToWorld(bodyList.First.Value);
            headVisual.transform.rotation = Quaternion.Euler(0, 0, DirectionToAngle(currentDirection));
        }

        // 给一节身体贴图/旋转赋值。headSideDir = 指向头侧邻居的方向，tailSideDir = 指向尾侧邻居的方向（尾巴时无意义，传 Vector2Int.zero 即可）
        private void ApplySegmentVisual(SpriteRenderer sr, Vector2Int headSideDir, Vector2Int tailSideDir, bool isTail)
        {
            if (isTail)
            {
                sr.sprite = trailImage;
                sr.flipX = false;
                sr.flipY = false;
                sr.transform.rotation = Quaternion.Euler(0, 0, TrailAngle(headSideDir));
            }
            else if (headSideDir == -tailSideDir)
            {
                sr.sprite = bodyImage;
                sr.flipX = false;
                sr.flipY = false;
                sr.transform.rotation = Quaternion.Euler(0, 0, TrailAngle(headSideDir));
            }
            else
            {
                sr.sprite = turnPointImage;
                sr.transform.rotation = Quaternion.identity;
                ApplyTurnFlip(sr, headSideDir, tailSideDir);
            }
        }

        // 新尾巴（原倒数第二节）头侧方向
        private Vector2Int GetTailHeadSideDirection()
        {
            var tailNode = bodyList.Last;
            if (tailNode == null || tailNode.Previous == null) return currentDirection;
            return tailNode.Previous.Value - tailNode.Value;
        }

        // 前进一格、不增长：把链表尾部对象摘下复用，变成新长出的第一节身体；
        private void AdvanceVisuals(Vector2Int oldHeadPos)
        {
            UpdateHeadVisual();
            if (bodyVisuals.Count == 0) return;

            GameObject reused = bodyVisuals.Last.Value;
            bodyVisuals.RemoveLast();

            bool becomesOnlySegment = bodyVisuals.Count == 0; // 摘掉之后链表空了，说明这一节移动后是唯一一节身体，身份应该是尾巴
            bodyVisuals.AddFirst(reused);

            reused.transform.position = gridManager.GridToWorld(oldHeadPos);
            SpriteRenderer reusedSr = reused.GetComponent<SpriteRenderer>();
            if (reusedSr != null)
            {
                // 新的第一节身体：头侧方向是这一帧的移动方向，尾侧方向是上一帧移动方向的反方向
                ApplySegmentVisual(reusedSr, currentDirection, -previousDirection, becomesOnlySegment);
            }

            if (!becomesOnlySegment)
            {
                GameObject newTailGo = bodyVisuals.Last.Value;
                SpriteRenderer tailSr = newTailGo.GetComponent<SpriteRenderer>();
                if (tailSr != null)
                {
                    ApplySegmentVisual(tailSr, GetTailHeadSideDirection(), Vector2Int.zero, isTail: true);
                }
            }
        }

        // 前进一格、吃到食物增长：只在旧蛇头位置新建一节身体，插到链表头部；
        private void GrowVisuals(Vector2Int oldHeadPos)
        {
            UpdateHeadVisual();

            bool wasEmpty = bodyVisuals.Count == 0; // 原来身体只有0节（蛇长为1），新长出的这节同时也是尾巴
            GameObject newSegment = Instantiate(bodyPrefab);
            bodyVisuals.AddFirst(newSegment);

            newSegment.transform.position = gridManager.GridToWorld(oldHeadPos);
            SpriteRenderer sr = newSegment.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                ApplySegmentVisual(sr, currentDirection, -previousDirection, wasEmpty);
            }
        }
        
        private void RenderBodyFull()
        {
            if (gridManager == null) return;

            UpdateHeadVisual();
            ClearBodyVisuals();

            List<Vector2Int> nodes = new List<Vector2Int>(bodyList);
            int tailIndex = nodes.Count - 1;

            for (int i = 1; i < nodes.Count; i++)
            {
                GameObject go = Instantiate(bodyPrefab);
                bodyVisuals.AddLast(go);

                go.transform.position = gridManager.GridToWorld(nodes[i]);

                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                if (sr == null) continue;

                Vector2Int headSideDir = nodes[i - 1] - nodes[i];
                Vector2Int tailSideDir = i < tailIndex ? nodes[i + 1] - nodes[i] : -headSideDir;

                ApplySegmentVisual(sr, headSideDir, tailSideDir, isTail: i == tailIndex);
            }
        }

        // 直行/尾部的旋转：Up→0° Down→180° Left→90° Right→-90°
        private float TrailAngle(Vector2Int dir)
        {
            return DirectionToAngle(dir);
        }
        
        // 基准图（不翻转）对应 headSideDir/tailSideDir = {left, down} 这一组合
        // （等价于：向右移动转向下 或 向上移动转向左）。
        // headSideDir、tailSideDir 在转弯处必然一横一竖，所以只需分别判断
        // 这一横一竖里是否出现了 right / up：
        //   出现 right → 水平翻转（对应基准里的 left）
        //   出现 up    → 垂直翻转（对应基准里的 down）
        private void ApplyTurnFlip(SpriteRenderer sr, Vector2Int headSideDir, Vector2Int tailSideDir)
        {
            bool hasRight = headSideDir == Vector2Int.right || tailSideDir == Vector2Int.right;
            bool hasUp = headSideDir == Vector2Int.up || tailSideDir == Vector2Int.up;

            sr.flipX = hasRight;
            sr.flipY = hasUp;
        }

        private float DirectionToAngle(Vector2Int dir)
        {
            if (dir == Vector2Int.up)    return 0f;
            if (dir == Vector2Int.down)  return 180f;
            if (dir == Vector2Int.left)  return 90f;
            if (dir == Vector2Int.right) return -90f;
            return 0f;
        }

        private void ClearBodyVisuals()
        {
            foreach (var go in bodyVisuals)
                if (go != null) Destroy(go);
            bodyVisuals.Clear();
        }

        public void ClearVisuals()
        {
            if (headVisual != null) { Destroy(headVisual); headVisual = null; }
            ClearBodyVisuals();
        }

        #endregion
    }
}