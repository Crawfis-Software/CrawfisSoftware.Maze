using CrawfisSoftware.Collections.Graph;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Create a maze by explicitly setting each cell
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    public class MazeBuilderExplicit<N, E> : MazeBuilderAbstract<N, E>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">The width of the desired maze</param>
        /// <param name="height">The height of the desired maze</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        public MazeBuilderExplicit(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
            : base(width, height, nodeAccessor, edgeAccessor)
        {
        }

        /// <summary>
        /// Set the directions for this cell
        /// </summary>
        /// <param name="i">The column index</param>
        /// <param name="j">The row index</param>
        /// <param name="dirs"></param>
        public void SetCell(int i, int j, Direction dirs)
        {
            directions[i, j] = dirs;
        }

        /// <inheritdoc/>
        public override void CreateMaze(bool preserveExistingCells)
        {
        }
    }
}
