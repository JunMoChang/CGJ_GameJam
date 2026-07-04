using UnityEngine;

namespace Gameplay
{
    public class CameraFitter : MonoBehaviour
    {
        [SerializeField] private float padding = 1f;

        public void FitToGrid(Vector2Int gridSize, float cellSize)
        {
            var cam = Camera.main;
            if (cam == null) return;

            float gridWidth = gridSize.x * cellSize;
            float gridHeight = gridSize.y * cellSize;
            float aspect = (float)Screen.width / Screen.height;

            // 取水平和垂直中较大的那一边
            cam.orthographicSize = Mathf.Max(
                gridHeight / 2f + padding,
                gridWidth / (2f * aspect) + padding);

            // 网格居中，相机也居中
            cam.transform.position = new Vector3(0, 0, -10f);
        }
    }
}
