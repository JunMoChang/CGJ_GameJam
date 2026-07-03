namespace Grid
{
    /// <summary>
    /// 网格格子的占用状态
    /// </summary>
    public enum GridCellState
    {
        Empty,
        SnakeHead,
        SnakeBody,
        Food,
        Obstacle
    }
}
