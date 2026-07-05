using UnityEngine;

namespace Config
{
    [CreateAssetMenu(menuName = "MaoMao/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Header("关卡基础")]
        public int level;
        public Vector2Int gridSize = new(8, 8);
        public float moveInterval = 1f;

        [Header("蛇初始")]
        public int initialSnakeLength = 3;

        [Header("初始岩石")]
        public int minInitialObstacles;
        public int maxInitialObstacles;

        [Header("随机岩石（第5关）")]
        public bool enableRandomObstacleSpawn;
        [Range(0f, 1f)] public float randomObstacleChance = 0.1f;
        [Range(0f, 1f)] 
        [Tooltip("障碍物占剩余空格子的最大占比")]
        public float maxObstacleProportionOfEmptyGrid = 0.2f;
        public int safeZoneSize = 5;

        [Header("教学")]
        public bool enableAttackTutorial;
    }
}
