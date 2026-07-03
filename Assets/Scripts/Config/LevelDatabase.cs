using UnityEngine;

namespace Config
{
    /// <summary>
    /// 管理所有关卡配置
    /// </summary>
    [CreateAssetMenu(menuName = "MaoMao/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        public LevelConfig[] levels;

        public LevelConfig GetLevel(int levelIndex)
        {
            foreach (var lv in levels)
            {
                if (lv != null && lv.levelIndex == levelIndex)
                    return lv;
            }
            return null;
        }

        public int LevelCount => levels?.Length ?? 0;
    }
}
