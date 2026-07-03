using UnityEngine;

namespace Grid
{
    /// <summary>
    /// 四向移动方向，附工具方法
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public static class DirectionUtils
    {
        public static Vector2Int ToVector2Int(Direction dir)
        {
            return dir switch
            {
                Direction.Up    => Vector2Int.up,
                Direction.Down  => Vector2Int.down,
                Direction.Left  => Vector2Int.left,
                Direction.Right => Vector2Int.right,
                _ => Vector2Int.zero
            };
        }

        public static bool IsOpposite(Direction a, Direction b)
        {
            return a switch
            {
                Direction.Up    => b == Direction.Down,
                Direction.Down  => b == Direction.Up,
                Direction.Left  => b == Direction.Right,
                Direction.Right => b == Direction.Left,
                _ => false
            };
        }

        public static float ToZRotation(Direction dir)
        {
            return dir switch
            {
                Direction.Up    => 0f,
                Direction.Down  => 180f,
                Direction.Left  => 90f,
                Direction.Right => -90f,
                _ => 0f
            };
        }

        /// <summary>从 Vector2 输入转为最近的 Direction（斜向取绝对值大的轴）</summary>
        public static Direction FromVector2(Vector2 input)
        {
            if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
                return input.x > 0 ? Direction.Right : Direction.Left;
            else
                return input.y > 0 ? Direction.Up : Direction.Down;
        }
    }
}
