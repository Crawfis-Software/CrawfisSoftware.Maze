using CrawfisSoftware.Collections.Graph;

namespace CrawfisSoftware.Maze
{
    /// <summary>
    /// Create a maze by explicitly setting each cell
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    [System.Obsolete("Use MazeBuilder or IMazeBuilder instead")]
    public class MazeBuilderExplicit<N, E> : MazeBuilderAbstract<N, E>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">The width of the desired maze</param>
        /// <param name="height">The height of the desired maze</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        [System.Obsolete("Use MazeBuilder or IMazeBuilder instead")]
        public MazeBuilderExplicit(int width, int height, GetGridLabel<N> nodeAccessor = null, GetEdgeLabel<E> edgeAccessor = null)
            : base(width, height, nodeAccessor, edgeAccessor)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="directions">A 2D array of Direction's to initialize the mazeBuilder from.</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        [System.Obsolete("Use MazeBuilder or IMazeBuilder instead")]
        public MazeBuilderExplicit(Direction[,] directions, GetGridLabel<N> nodeAccessor = null, GetEdgeLabel<E> edgeAccessor = null)
            : base(directions.GetLength(0), directions.GetLength(1), nodeAccessor, edgeAccessor)
        {
            for (int i = 0; i < directions.GetLength(0); i++)
            {
                for (int j = 0; j < directions.GetLength(1); j++)
                {
                    SetCell(i, j, directions[i, j]);
                }
            }
        }
    }
}
