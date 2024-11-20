using CrawfisSoftware.Collections.Graph;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Modify a maze based on maze metrics.
    /// </summary>
    // Todo: Add Generics <N, E> - requires MazeMetrics to be generic
    public class MazeBuilderModifiers<N, E> : MazeBuilderAbstract<N, E>
    {
        private readonly MazeMetricsComputations<N, E> _metricsComputations;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maze">The initial maze</param>
        /// <param name="metricsComputations">A set of maze metrics.</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        public MazeBuilderModifiers(Maze<N, E> maze, MazeMetricsComputations<N, E> metricsComputations, GetGridLabel<N> nodeAccessor = null, GetEdgeLabel<E> edgeAccessor = null) : base(maze.Width, maze.Height, nodeAccessor, edgeAccessor)
        {
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    this.SetCell(column, row, maze.GetDirection(column, row));
                }
            }
            _metricsComputations = metricsComputations;
        }

        /// <inheritdoc/>
        public override void CreateMaze(bool preserveExistingCells = false)
        {
        }

        /// <summary>
        /// Trim all dead-ends to a specified maximum length.
        /// </summary>
        /// <param name="maxDeadEndLength">Length in number of cells.</param>
        public void TrimDeadEnds(int maxDeadEndLength)
        {
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    var metrics = _metricsComputations.GetCellMetrics(column, row);
                    int? distance = metrics.PathDistanceToSolution;
                    if (distance.HasValue)
                    {
                        int cellsFromSolution = distance.Value;
                        if (cellsFromSolution > maxDeadEndLength)
                        {
                            // Find all cells > mazDeadEndLength and set to Direction.None (| Undefined?)
                            this.SetCell(column, row, GetDirection(column, row) & Direction.Undefined);
                        }
                        else if (cellsFromSolution == maxDeadEndLength)
                        {
                            // Find all cells == maxDeadEndLength and remove any Exits.
                            Direction entranceEdge = Direction.None;
                            if (metrics.LeftEdgeFlow == EdgeFlow.Entrance)
                                entranceEdge = Direction.W;
                            if (metrics.TopEdgeFlow == EdgeFlow.Entrance)
                                entranceEdge = Direction.N;
                            if (metrics.RightEdgeFlow == EdgeFlow.Entrance)
                                entranceEdge = Direction.E;
                            if (metrics.BottomEdgeFlow == EdgeFlow.Entrance)
                                entranceEdge = Direction.S;
                            this.SetCell(column, row, entranceEdge & Direction.Undefined);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Trim a specific dead-end to the specified maximum length.
        /// </summary>
        /// <param name="branchId">The solution path cell id.</param>
        /// <param name="maxDeadEndLength">Length in number of cells.</param>
        public void TrimDeadEnds(int branchId, int maxDeadEndLength)
        {
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    var metrics = _metricsComputations.GetCellMetrics(column, row);
                    var branch = metrics.BranchId;
                    int? distance = metrics.PathDistanceToSolution;
                    if (distance.HasValue && branch.HasValue && branch.Value.solutionPathCell == branchId)
                    {
                        int cellsFromSolution = distance.Value;
                        if (cellsFromSolution > maxDeadEndLength)
                        {
                            // Find all cells > mazDeadEndLength and set to Direction.None (| Undefined?)
                            this.SetCell(column, row, GetDirection(column, row) & Direction.Undefined);
                        }
                        else if (cellsFromSolution == maxDeadEndLength)
                        {
                            // Find all cells == maxDeadEndLength and remove any Exits.
                            Direction entranceEdge = Direction.None;
                            if (metrics.LeftEdgeFlow == EdgeFlow.Entrance)
                                entranceEdge = Direction.W;
                            if (metrics.TopEdgeFlow == EdgeFlow.Entrance)
                                entranceEdge = Direction.N;
                            if (metrics.RightEdgeFlow == EdgeFlow.Entrance)
                                entranceEdge = Direction.E;
                            if (metrics.BottomEdgeFlow == EdgeFlow.Entrance)
                                entranceEdge = Direction.S;
                            this.SetCell(column, row, entranceEdge & Direction.Undefined);
                        }
                    }
                }
            }
        }
    }
}