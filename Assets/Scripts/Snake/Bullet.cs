using System;
using System.Collections;
using UnityEngine;

namespace Snake
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private int maxRange = 2;
        [SerializeField] private GameObject trailPrefab;

        private Vector2Int direction;
        private float traveled;
        private bool destroyed;
        private Gameplay.ObstacleManager obstacleManager;
        private Grid.GridManager gridManager;
        private Action<Vector2Int> onDestroyObstacle;
        private GameObject trailInstance;

        public void Init(Vector2Int dir, Gameplay.ObstacleManager obsMgr, Grid.GridManager gm, Action<Vector2Int> onObstacleDestroyed)
        {
            direction = dir;
            obstacleManager = obsMgr;
            gridManager = gm;
            onDestroyObstacle = onObstacleDestroyed;
            traveled = 0f;
            
            if (trailPrefab != null)
            {
                trailInstance = Instantiate(trailPrefab, transform);
                trailInstance.transform.localPosition = Vector3.zero;
            }
        }

        private void Update()
        {
            if (destroyed) return;

            float move = speed * Time.deltaTime;
            transform.position += new Vector3(direction.x, direction.y, 0) * move;
            traveled += move;
            
            if (trailInstance != null)
            {
                Vector3 trailPos = trailInstance.transform.position;
                trailPos = Vector3.MoveTowards(trailPos, transform.position - new Vector3(direction.x, direction.y, 0), speed * Time.deltaTime);
                trailInstance.transform.position = trailPos;
            }

            if (gridManager != null)
            {
                Vector2Int currentCell = gridManager.WorldToGrid(transform.position);
                if (gridManager.GetState(currentCell) == Grid.GridCellState.Obstacle)
                {
                    obstacleManager?.DestroyObstacle(currentCell);
                    onDestroyObstacle?.Invoke(currentCell);
                    Destroy(gameObject);
                    destroyed = true;
                    return;
                }
            }

            if (traveled >= maxRange * gridManager.CellSize)
            {
                destroyed = true;
                StartCoroutine(DestroyAfterDelay(0.1f));
            }
        }
        
        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }
    }
}
