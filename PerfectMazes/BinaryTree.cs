using CrawfisSoftware.Collections.Graph;
using CrawfisSoftware.Collections.Maze;

namespace CrawfisSoftware.Maze
{
    /// <summary>
    /// Extensions for IMazeBuilder for various perfect maze (spanning tree) algorithms.
    /// </summary>
    // todo: .Net Standard 3.0 and .Net 8.0 support partial static classes.
    // When Unity supports 3.0 we can make these partial and have the name w/o numbers.
    public static /*partial*/ class PerfectMaze2
    {
        /// <summary>
        /// Create a maze using the Binary Tree algorithm
        /// </summary>
        /// <param name="mazeBuilder">A maze builder</param>
        /// <param name="percentHorizontal">Control to favor horizontal or vertical runs</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined. Default is false.</param>
        /// <typeparam name="N">The type used for node labels</typeparam>
        /// <typeparam name="E">The type used for edge weights</typeparam>
        public static void BinaryTree<N, E>(this IMazeBuilder<N, E> mazeBuilder, int percentHorizontal = 50, bool preserveExistingCells = false)
        {
            const int maxRandomValue = 1000;
            int threshold = (percentHorizontal * maxRandomValue) / 100; // Favor horizontal runs
            int row, column;
            var grid = mazeBuilder.Grid;
            IndexedGraphEnumerator<N, E> graphWalker = new IndexedGraphEnumerator<N, E>(grid);
            foreach (var node in graphWalker.TraverseNodes())
            {
                if (grid.TryGetGridLocation(node, out column, out row))
                {
                    bool moveEast = mazeBuilder.RandomGenerator.Next(maxRandomValue) < threshold;
                    bool eastBorder = false;
                    if (column >= mazeBuilder.Width - 1) eastBorder = true;
                    moveEast &= !eastBorder;
                    bool carved = false;
                    if (moveEast)
                    {
                        carved = mazeBuilder.CarveDirectionally(column, row, Direction.E, preserveExistingCells);
                    }
                    if (!carved && (row < (mazeBuilder.Height - 1)))
                    {
                        carved = mazeBuilder.CarveDirectionally(column, row, Direction.N, preserveExistingCells);
                    }
                    if (!carved && (row == (mazeBuilder.Height - 1)) && !eastBorder)
                    {
                        carved = mazeBuilder.CarveDirectionally(column, row, Direction.E, preserveExistingCells);
                    }
                    // Todo: Rewrite to have a list of possible choices in order (east, North), or (North, East), or (North), or (East) or ().
                    // Loop through each choice as long as carved is false.
                }
            }
        }
    }
}