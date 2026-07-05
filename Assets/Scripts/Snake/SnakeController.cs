using System;
using System.Collections.Generic;
using Config;
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
        [SerializeField] private Sprite bodyImage;
        [SerializeField] private Sprite trailImage;
        [SerializeField] private Sprite turnPointImage;
        [SerializeField] private GameObject bulletPrefab;
        
        [Header("攻击")]
        [SerializeField] private int attackRange = 2;
        [SerializeField] private float attackCooldown = 0.6f;

        [Header("引用")]
        [SerializeField] private Grid.GridManager gridManager;
        [SerializeField] private ObstacleManager obstacleManager;
        
        public event Action<int> OnLengthChanged;// 蛇长变化
        public event Action OnDead;//死亡
        public event Action<Vector2Int> OnMoved;//每次移动后，传蛇头坐标
        public event Action<Vector2Int> OnFoodEaten;//吃到食物，传食物坐标
        public event Action<Vector2Int> OnObstacleDestroyed;//攻击破坏岩石
        public event Action OnAttack;//攻击
        
        
        private readonly LinkedList<Vector2Int> bodyList = new();
        private readonly HashSet<Vector2Int> bodySet = new();
        private GameObject headVisual;
        private readonly LinkedList<GameObject> bodyVisuals = new(); // 与 bodyList 去掉表头后的部分一一对应

        private Vector2Int currentDirection = Vector2Int.right;
        private Vector2Int pendingDirection = Vector2Int.right;
        private Vector2Int previousDirection = Vector2Int.right;

        private float tickTimer;
        private float lastAttackTime;
        private bool isGameOver;
        
        public Vector2Int HeadPosition => bodyList.Count > 0 ? bodyList.First.Value : Vector2Int.zero;
        public IReadOnlyCollection<Vector2Int> BodyPositions => bodySet;
        public int Length => bodyList.Count;
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

        #region 输入

        private void ReadDirectionInput()
        {
            Vector2 raw = InputManager.Instance.GetVector2("Move");
            if (raw == Vector2.zero) return;
            
            Vector2Int candidate = Mathf.Abs(raw.x) >= Mathf.Abs(raw.y) ? raw.x > 0 ? Vector2Int.right : Vector2Int.left : raw.y > 0 ? Vector2Int.up : Vector2Int.down;
            
            if (candidate == -currentDirection) return;//禁止180转弯

            pendingDirection = candidate;
        }
        
        private void HandleInputPressed(string actionName)
        {
            if (isGameOver) return;
            if (actionName != "Attack") return;

            LevelConfig cfg = Core.GameManager.Instance?.CurrentLevelConfig;
            if (cfg == null || !cfg.enableAttack) return;

            TryAttack();
        }

        #endregion

        #region Tick

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
            
            gridManager.SetState(oldHeadPos, Grid.GridCellState.SnakeBody);

            bodyList.AddFirst(newHead);
            bodySet.Add(newHead);

            gridManager.SetState(newHead, Grid.GridCellState.SnakeHead);

            // 移除尾部或增长
            if (!ateFood)
            {
                Vector2Int removedPos= bodyList.Last.Value;
                bodyList.RemoveLast();
                bodySet.Remove(removedPos);
                gridManager.SetState(removedPos, Grid.GridCellState.Empty);

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
            
            if (Time.time - lastAttackTime < attackCooldown) return;

            lastAttackTime = Time.time;

            if (bulletPrefab == null) return;

            Vector3 spawnPos = gridManager.GridToWorld(bodyList.First.Value);
            OnAttack?.Invoke();

            GameObject bulletGo = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            Bullet bullet = bulletGo.GetComponent<Bullet>();
            if (bullet != null) bullet.Init(currentDirection, obstacleManager, gridManager, pos => OnObstacleDestroyed?.Invoke(pos));
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

        // 头部视觉
        private void UpdateHeadVisual()
        {
            if (headVisual == null) headVisual = Instantiate(headPrefab);

            headVisual.transform.position = gridManager.GridToWorld(bodyList.First.Value);
            headVisual.transform.rotation = Quaternion.Euler(0, 0, DirectionToAngle(currentDirection));
        }

        // 给一节身体贴图/旋转赋值。headSideDir = 指向头侧邻居的方向，tailSideDir = 指向尾侧邻居的方向
        private void ApplySegmentVisual(SpriteRenderer sr, Vector2Int headSideDir, Vector2Int tailSideDir, bool isTail)
        {
            if (isTail)
            {
                sr.sprite = trailImage;
                sr.flipX = false;
                sr.flipY = false;
                sr.transform.rotation = Quaternion.Euler(0, 0, DirectionToAngle(headSideDir));
            }
            else if (headSideDir == -tailSideDir)
            {
                sr.sprite = bodyImage;
                sr.flipX = false;
                sr.flipY = false;
                sr.transform.rotation = Quaternion.Euler(0, 0, DirectionToAngle(headSideDir));
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
            LinkedListNode<Vector2Int> tailNode = bodyList.Last;
            if (tailNode == null || tailNode.Previous == null) return currentDirection;
            return tailNode.Previous.Value - tailNode.Value;
        }

        // 把链表尾部对象摘下复用，变成新长出的第一节身体
        private void AdvanceVisuals(Vector2Int oldHeadPos)
        {
            UpdateHeadVisual();
            if (bodyVisuals.Count == 0) return;

            GameObject reusedBody = bodyVisuals.Last.Value;
            bodyVisuals.RemoveLast();

            bool becomesOnlySegment = bodyVisuals.Count == 0;
            bodyVisuals.AddFirst(reusedBody);

            reusedBody.transform.position = gridManager.GridToWorld(oldHeadPos);
            SpriteRenderer reusedSr = reusedBody.GetComponent<SpriteRenderer>();
            if (reusedSr != null)
            {
                ApplySegmentVisual(reusedSr, currentDirection, -previousDirection, becomesOnlySegment);
            }

            if (!becomesOnlySegment)
            {
                GameObject newTail = bodyVisuals.Last.Value;
                SpriteRenderer tailSr = newTail.GetComponent<SpriteRenderer>();
                if (tailSr != null)
                {
                    ApplySegmentVisual(tailSr, GetTailHeadSideDirection(), Vector2Int.zero, isTail: true);
                }
            }
        }

        // 在旧蛇头位置新建一节身体，插到链表头部
        private void GrowVisuals(Vector2Int oldHeadPos)
        {
            UpdateHeadVisual();

            bool wasEmpty = bodyVisuals.Count == 0;
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
                GameObject body = Instantiate(bodyPrefab);
                bodyVisuals.AddLast(body);

                body.transform.position = gridManager.GridToWorld(nodes[i]);

                SpriteRenderer sr = body.GetComponent<SpriteRenderer>();
                if (sr == null) continue;

                Vector2Int headSideDir = nodes[i - 1] - nodes[i];
                Vector2Int tailSideDir = i < tailIndex ? nodes[i + 1] - nodes[i] : -headSideDir;

                ApplySegmentVisual(sr, headSideDir, tailSideDir, isTail: i == tailIndex);
            }
        }
        
        // 基准图对应 headSideDir/tailSideDir = {right, down}
        private void ApplyTurnFlip(SpriteRenderer sr, Vector2Int headSideDir, Vector2Int tailSideDir)
        {
            bool hasRight = headSideDir == Vector2Int.right || tailSideDir == Vector2Int.right;
            bool hasUp = headSideDir == Vector2Int.up || tailSideDir == Vector2Int.up;

            sr.flipX = hasRight;
            sr.flipY = hasUp;
        }

        private float DirectionToAngle(Vector2Int dir)
        {
            if (dir == Vector2Int.up) return 0f;
            if (dir == Vector2Int.down) return 180f;
            if (dir == Vector2Int.left) return 90f;
            if (dir == Vector2Int.right) return -90f;
            return 0f;
        }

        private void ClearBodyVisuals()
        {
            foreach (GameObject go in bodyVisuals)
            {
                if (go != null) Destroy(go);
            }
            
            bodyVisuals.Clear();
        }

        public void ClearVisuals()
        {
            if (headVisual != null)
            {
                Destroy(headVisual);
                headVisual = null;
            }
            
            ClearBodyVisuals();
        }

        #endregion
    }
}