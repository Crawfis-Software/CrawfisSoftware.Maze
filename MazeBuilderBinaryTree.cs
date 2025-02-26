using CrawfisSoftware.Collections.Graph;
using CrawfisSoftware.Maze;

namespace CrawfisSoftware.Maze
{
    /// <summary>
    /// Create a maze using the Binary Tree algorithm
    /// </summary>
    /// <typeparam name="N"></typeparam>
    /// <typeparam name="E"></typeparam>
    //public class MazeBuilderBinaryTree<N, E>
    //{
    //    private MazeBuilderAbstract<N, E> _mazeBuilder;
    //    private int percentHorizontal = 50;
    //    /// <summary>
    //    /// Control to favor horizontal or vertical runs
    //    /// </summary>
    //    public int PercentHorizontal
    //    {
    //        get { return percentHorizontal; }
    //        set
    //        {
    //            percentHorizontal = value;
    //            if (value < 0) percentHorizontal = 0;
    //            if (value > 100) percentHorizontal = 100;
    //        }
    //    }

    //    /// <summary>
    //    /// Constructor, Takes an existing maze builder (derived from MazeBuilderAbstract) and copies the state over.
    //    /// </summary>
    //    /// <param name="mazeBuilder">A maze builder</param>
    //    /// <param name="percentHorizontal">Control to favor horizontal or vertical runs</param>
    //    public MazeBuilderBinaryTree(MazeBuilderAbstract<N, E> mazeBuilder, int percentHorizontal = 50)
    //    {
    //        _mazeBuilder = mazeBuilder;
    //        PercentHorizontal = percentHorizontal;
    //    }

    //    /// <summary>
    //    /// Create a maze using the Binary Tree algorithm
    //    /// </summary>
    //    /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined. Default is false.</param>
    //    public void CreateMaze(bool preserveExistingCells = false)
    //    {
    //        // Todo: Throw an exception if preserveExistingCells = true;
    //        BinaryTreeMaze(preserveExistingCells);
    //    }

    //    private void BinaryTreeMaze(bool preserveExistingCells)
    //    {
    //        const int maxRandomValue = 1000;
    //        int threshold = (PercentHorizontal * maxRandomValue) / 100; // Favor horizontal runs
    //        int row, column;
    //        var grid = _mazeBuilder.Grid;
    //        IndexedGraphEnumerator<N, E> graphWalker = new IndexedGraphEnumerator<N, E>(grid);
    //        foreach (var node in graphWalker.TraverseNodes())
    //        {
    //            if (grid.TryGetGridLocation(node, out column, out row))
    //            {
    //                bool moveEast = _mazeBuilder.RandomGenerator.Next(maxRandomValue) < threshold;
    //                bool eastBorder = false;
    //                if (column >= _mazeBuilder.Width - 1) eastBorder = true;
    //                moveEast &= !eastBorder;
    //                bool carved = false;
    //                if (moveEast)
    //                {
    //                    carved = _mazeBuilder.CarveDirectionally(column, row, Direction.E, preserveExistingCells);
    //                }
    //                if (!carved && (row < (_mazeBuilder.Height - 1)))
    //                {
    //                    carved = _mazeBuilder.CarveDirectionally(column, row, Direction.N, preserveExistingCells);
    //                }
    //                if (!carved && (row == (_mazeBuilder.Height - 1)) && !eastBorder)
    //                {
    //                    carved = _mazeBuilder.CarveDirectionally(column, row, Direction.E, preserveExistingCells);
    //                }
    //                // Todo: Rewrite to have a list of possible choices in order (east, North), or (North, East), or (North), or (East) or ().
    //                // Loop through each choice as long as carved is false.
    //            }
    //        }
    //    }
    //}
}
