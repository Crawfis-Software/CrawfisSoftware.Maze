using CrawfisSoftware.Collections.Graph;
using System;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Craft a "maze" by carving specific paths.
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    public class MazeBuilderShortestPaths<N, E> : MazeBuilderAbstract<N, E>
    {
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
        public MazeBuilderShortestPaths(MazeBuilderAbstract<N, E> mazeBuilder) : base(mazeBuilder)
        {
                EdgeFunction = ConstantOfOne;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="width">The width of the desired maze</param>
        /// <param name="height">The height of the desired maze</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        public MazeBuilderShortestPaths(int width, int height, GetGridLabel<N> nodeAccessor = null, GetEdgeLabel<E> edgeAccessor = null) : base(width, height, nodeAccessor, edgeAccessor)
        {
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
            foreach (var cell in PathQuery<N, E>.FindPath(grid, startingCell, endingCell, EdgeComparerUsingGetEdgeLabel))
            {
                CarvePassage(cell.From, cell.To, false);
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

        /// <inheritdoc/>
        public override void CreateMaze(bool preserveExistingCells = false)
        {
        }

        private float EdgeComparerUsingGetEdgeLabel(IIndexedEdge<E> edge)
        {
            return EdgeFunction(edge, directions[edge.From % Width, edge.From / Width], directions[edge.To % Width, edge.To / Width]);
        }

        private void CarveShortestPaths(bool preserveExistingCells, int targetCell, float maxCost = float.MaxValue)
        {
            var pathQuery = new Graph.SourceShortestPaths<N, E>(grid, targetCell, EdgeComparerUsingGetEdgeLabel);
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    int targetNode = column + row * Width;
                    if (pathQuery.GetCost(targetNode) >= maxCost) continue;
                    foreach (var cell in pathQuery.GetPath(targetNode))
                    {
                        CarvePassage(cell.From, cell.To, false);
                    }
                }
            }
        }
    }
}