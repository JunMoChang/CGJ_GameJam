using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// 根据网格尺寸自适应相机 orthographicSize
    /// </summary>
    public class CameraFitter : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float padding = 1f;

        public void FitToGrid(Vector2Int gridSize, float cellSize)
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            float width = gridSize.x * cellSize;
            float height = gridSize.y * cellSize;
            float aspect = (float)Screen.width / Screen.height;

            cam.orthographicSize = Mathf.Max(height / 2f + padding, width / (2f * aspect) + padding);
            cam.transform.position = new Vector3(
                width / 2f - cellSize / 2f,
                height / 2f - cellSize / 2f,
                -10f);
        }
    }
}
