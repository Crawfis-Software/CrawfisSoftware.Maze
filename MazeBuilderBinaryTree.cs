using CrawfisSoftware.Collections.Graph;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Create a maze using the Binary Tree algorithm
    /// </summary>
    /// <typeparam name="N"></typeparam>
    /// <typeparam name="E"></typeparam>
    public class MazeBuilderBinaryTree<N, E> : MazeBuilderAbstract<N, E>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">The width of the desired maze</param>
        /// <param name="height">The height of the desired maze</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        public MazeBuilderBinaryTree(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor) : base(width, height, nodeAccessor, edgeAccessor)
        {
        }
        private void BinaryTreeMaze(int percentHorizontal = 50)
        {
            // Sidewinder
            // Initial state is that all directions are zero.
            const int maxRandomValue = 1000;
            if (percentHorizontal < 0) percentHorizontal = 0;
            if (percentHorizontal > 100) percentHorizontal = 100;
            int threshold = (percentHorizontal * maxRandomValue) / 100; // Favor horizontal runs
            int row, column;
            IndexedGraphEnumerator<N, E> graphWalker = new IndexedGraphEnumerator<N, E>(grid);
            foreach (var node in graphWalker.TraverseNodes())
            {
                if (grid.TryGetGridLocation(node, out column, out row))
                {
                    bool moveEast = RandomGenerator.Next(maxRandomValue) < threshold;
                    bool eastBorder = false;
                    if (column >= Width - 1) eastBorder = true;
                    moveEast &= !eastBorder;
                    if (moveEast)
                    {
                        directions[column, row] |= Direction.E;
                        if (column + 1 <= Width - 1) directions[column + 1, row] |= Direction.W;
                    }
                    else if (row < Height - 1)
                    {
                        directions[column, row] |= Direction.N;
                        directions[column, row + 1] |= Direction.S;
                    }
                    else if ((row == Height - 1) && !eastBorder)
                    {
                        directions[column, row] |= Direction.E;
                        if (column + 1 <= Width - 1) directions[column + 1, row] |= Direction.W;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void CreateMaze(bool preserveExistingCells = false)
        {
            // Todo: Throw an exception if preserveExistingCells = true;
            BinaryTreeMaze();
        }
    }
}
