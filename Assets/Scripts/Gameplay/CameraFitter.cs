using UnityEngine;

namespace Gameplay
{
    public class CameraFitter : MonoBehaviour
    {
        [SerializeField] private Vector2Int maxGridSize = new(14, 14);
        [SerializeField] private float padding = 0.6f;

        public void FitToGrid(Vector2Int gridSize, float cellSize)
        {
            var cam = Camera.main;
            if (cam == null) return;

            float gridWidth = maxGridSize.x * cellSize;
            float gridHeight = maxGridSize.y * cellSize;
            float aspect = (float)Screen.width / Screen.height;

            cam.orthographicSize = Mathf.Max(
                gridHeight / 2f + padding,
                gridWidth / (2f * aspect) + padding);

            cam.transform.position = new Vector3(0, 0, -10f);
        }
    }
}
