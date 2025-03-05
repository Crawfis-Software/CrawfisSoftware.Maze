using CrawfisSoftware.Collections.Graph;

using System;

namespace CrawfisSoftware.Maze
{
    /// <summary>
    /// Craft a "maze" by carving specific paths.
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    public class MazeBuilderShortestPaths<N, E>
    {
        private IMazeBuilder<N, E> _mazeBuilder;
        /// <summary>
        /// Static function that can be assigned to the EdgeFunction. This one just returns the edge's value when the Edge Type is a float.
        /// </summary>
        /// <param name="edge">The indexed edge.</param>
        /// <param name="fromCell">The (current) set of directions the "from" cell has.</param>
        /// <param name="toCell">The (current) set of directions the "to" cell has.</param>
        /// <returns>A float value to use as the edge value.</returns>
        public static float EdgeComparerUsingGetEdgeLabel(IIndexedEdge<float> edge, Direction fromCell, Direction toCell)
        {
            return edge.Value;
        }

        /// <summary>
        /// Static function that can be assigned to the EdgeFunction. This one just returns the edge's value when the Edge Type is a int (as a float).
        /// </summary>
        /// <param name="edge">The indexed edge.</param>
        /// <param name="fromCell">The (current) set of directions the "from" cell has.</param>
        /// <param name="toCell">The (current) set of directions the "to" cell has.</param>
        /// <returns>A float value to use as the edge value.</returns>
        public static float EdgeComparerUsingGetEdgeLabel(IIndexedEdge<int> edge, Direction fromCell, Direction toCell)
        {
            return edge.Value;
        }

        /// <summary>
        /// Static function that can be assigned to the EdgeFunction. This one just returns the value of one as a float.
        /// </summary>
        /// <param name="edge">The indexed edge.</param>
        /// <param name="fromCell">The (current) set of directions the "from" cell has.</param>
        /// <param name="toCell">The (current) set of directions the "to" cell has.</param>
        /// <returns>The floating value 1.0f.</returns>
        public static float ConstantOfOne(IIndexedEdge<E> edge, Direction fromCell, Direction toCell)
        {
            return 1;
        }

        /// <summary>
        /// A function that takes the edge and the two cells current set of maze directions and returns a float.
        /// </summary>
        public Func<IIndexedEdge<E>, Direction, Direction, float> EdgeFunction { get; set; }

        /// <summary>
        /// Constructor initialized with a prior MazeBuilder.
        /// </summary>
        /// <param name="mazeBuilder">A maze builder.</param>
        public MazeBuilderShortestPaths(IMazeBuilder<N, E> mazeBuilder)
        {
            _mazeBuilder = mazeBuilder;
            EdgeFunction = ConstantOfOne;
        }

        /// <summary>
        /// Carves a path from the starting cell to the ending cell.
        /// </summary>
        /// <param name="startingCell">The index of the starting cell.</param>
        /// <param name="endingCell">The index of the ending cell.</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        public void CarvePath(int startingCell, int endingCell, bool preserveExistingCells = false)
        {
            foreach (var cell in PathQuery<N, E>.FindPath(_mazeBuilder.Grid, startingCell, endingCell, EdgeComparerUsingGetEdgeLabel))
            {
                _mazeBuilder.CarvePassage(cell.From, cell.To, preserveExistingCells);
            }
        }

        /// <summary>
        /// Carves path to every node in the underlying grid from the target cell that are reachable for a fixed cost.
        /// </summary>
        /// <param name="targetCell">A grid cell index.</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        /// <param name="maxCost">The maximum cost that a node is reachable.</param>
        public void CarveAllShortestPathsToTarget(int targetCell, bool preserveExistingCells = false, float maxCost = float.MaxValue)
        {
            CarveShortestPaths(preserveExistingCells, targetCell, maxCost);

        }

        private float EdgeComparerUsingGetEdgeLabel(IIndexedEdge<E> edge)
        {
            int width = _mazeBuilder.Width;
            return EdgeFunction(edge, _mazeBuilder.GetDirection(edge.From % width, edge.From / width), _mazeBuilder.GetDirection(edge.To % width, edge.To / width));
        }

        private void CarveShortestPaths(bool preserveExistingCells, int targetCell, float maxCost = float.MaxValue)
        {
            int width = _mazeBuilder.Width;
            int height = _mazeBuilder.Height;
            var pathQuery = new SourceShortestPaths<N, E>(_mazeBuilder.Grid, targetCell, EdgeComparerUsingGetEdgeLabel);
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    int targetNode = column + row * width;
                    if (pathQuery.GetCost(targetNode) >= maxCost) continue;
                    foreach (var cell in pathQuery.GetPath(targetNode))
                    {
                        _mazeBuilder.CarvePassage(cell.From, cell.To, preserveExistingCells);
                    }
                }
            }
        }
    }
}