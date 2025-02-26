using CrawfisSoftware.Collections.Graph;
using CrawfisSoftware.Maze;

namespace CrawfisSoftware.Maze
{
    /// <summary>
    /// A path cost comparer for a Maze. The cost of traversing a passage is PassageTraversalCost.
    /// </summary>
    /// <typeparam name="N">The type of the node labels in the corresponding graph.</typeparam>
    /// <typeparam name="E">The type of the edge labels in the corresponding graph.</typeparam>
    public class MazePathCost<N, E> : GridPathComparer<N, E>
    {
        private IMazeBuilder<N, E> _maze;
        int _width;

        /// <summary>
        /// The cost of traversing an existing passage.
        /// </summary>
        public float PassageTraversalCost { get; set; } = 1.0f;
        /// <summary>
        /// The cost of carving a previously defined wall.
        /// </summary>
        public float FixedWallCarveCost { get; set; } = 10000.0f;
        /// <summary>
        /// The cost of carving unexplored space.
        /// </summary>
        public float UndefinedWallCarveCost { get; set; } = 10.0f;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mazeBuilder">The underlying maze builder, <c>MazeBuilderAbstract</c>.</param>
        public MazePathCost(IMazeBuilder<N, E> mazeBuilder) : base(mazeBuilder.Grid, null, 1)
        {
            _maze = mazeBuilder;
            _width = mazeBuilder.Width;
            this.EdgeCostDelegate = EdgeCost;
        }

        private float EdgeCost(IIndexedEdge<E> edge)
        {
            int row1 = edge.From / _width;
            int col1 = edge.From % _width;
            int row2 = edge.To / _width;
            int col2 = edge.To % _width;
            Direction cellDirs = _maze.GetDirection(col1, row1);
            Direction edgeDir = DirectionExtensions.GetEdgeDirection(edge.From, edge.To, _width);
            if ((cellDirs & edgeDir) == edgeDir)
                return PassageTraversalCost;
            else if ((cellDirs & Direction.Undefined) != Direction.Undefined)
                return UndefinedWallCarveCost;
            else
                return FixedWallCarveCost;
        }
    }
}