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
        private int percentHorizontal = 50;
        /// <summary>
        /// Control to favor horizontal or vertical runs
        /// </summary>
        public int PercentHorizontal
        {
            get { return percentHorizontal; }
            set
            {
                percentHorizontal = value;
                if (value < 0) percentHorizontal = 0;
                if (value > 100) percentHorizontal = 100;
            }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">The width of the desired maze</param>
        /// <param name="height">The height of the desired maze</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        /// <param name="percentHorizontal">Control to favor horizontal or vertical runs</param>
        public MazeBuilderBinaryTree(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor, int percentHorizontal = 50) : base(width, height, nodeAccessor, edgeAccessor)
        {
            PercentHorizontal = percentHorizontal;
        }

        /// <summary>
        /// Constructor, Takes an existing maze builder (derived from MazeBuilderAbstract) and copies the state over.
        /// </summary>
        /// <param name="mazeBuilder">A maze builder</param>
        /// <param name="percentHorizontal">Control to favor horizontal or vertical runs</param>
        public MazeBuilderBinaryTree(MazeBuilderAbstract<N, E> mazeBuilder, int percentHorizontal = 50) : base(mazeBuilder)
        {
            PercentHorizontal = percentHorizontal;
        }

        private void BinaryTreeMaze(bool preserveExistingCells)
        {
            // Sidewinder
            // Initial state is that all directions are zero.
            const int maxRandomValue = 1000;
            int threshold = (PercentHorizontal * maxRandomValue) / 100; // Favor horizontal runs
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
                    bool carved = false;
                    if (moveEast)
                    {
                        carved = CarveDirectionally(column, row, Direction.E, preserveExistingCells);
                        //directions[column, row] |= Direction.E;
                        //if (column + 1 <= Width - 1) directions[column + 1, row] |= Direction.W;
                    }
                    if (!carved && (row < (Height - 1)))
                    {
                        carved = CarveDirectionally(column, row, Direction.N, preserveExistingCells);
                        //directions[column, row] |= Direction.N;
                        //directions[column, row + 1] |= Direction.S;
                    }
                    if (!carved && (row == (Height - 1)) && !eastBorder)
                    {
                        carved = CarveDirectionally(column, row, Direction.E, preserveExistingCells);
                        //    //directions[column, row] |= Direction.E;
                        //    //if (column + 1 <= Width - 1) directions[column + 1, row] |= Direction.W;
                    }
                    // Todo: Rewrite to have a list of possible choices in order (east, North), or (North, East), or (North), or (East) or ().
                    // Loop through each choice as long as carved is false.
                }
            }
        }

        /// <inheritdoc/>
        public override void CreateMaze(bool preserveExistingCells = false)
        {
            // Todo: Throw an exception if preserveExistingCells = true;
            BinaryTreeMaze(preserveExistingCells);
            // Clear all Undefined flags, since maze generation should touch all cells.
            // Todo: Not true, as "grid" may be masked to certain edges.
            RemoveUndefines();
        }
    }
}
