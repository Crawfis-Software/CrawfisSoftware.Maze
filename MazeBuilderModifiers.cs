using CrawfisSoftware.Collections.Graph;

namespace CrawfisSoftware.Maze
{
    /// <summary>
    /// Modify a Maze Builder based on maze metrics.
    /// </summary>
    public static class MazeBuilderModifiers
    {
        /// <summary>
        /// Trim all dead-ends to a specified maximum length.
        /// </summary>
        /// <param name="mazeBuilder">The maze builder to modify.</param>
        /// <param name="metricsComputations">The metrics computations for the maze.</param>
        /// <param name="maxDeadEndLength">Length in number of cells.</param>
        public static void TrimDeadEnds<N, E>(this IMazeBuilder<N, E> mazeBuilder, MazeMetricsComputations<N, E> metricsComputations, int maxDeadEndLength)
        {
            for (int row = 0; row < mazeBuilder.Height; row++)
            {
                for (int column = 0; column < mazeBuilder.Width; column++)
                {
                    var metrics = metricsComputations.GetCellMetrics(column, row);
                    int? distance = metrics.PathDistanceToSolution;
                    if (distance.HasValue)
                    {
                        int cellsFromSolution = distance.Value;
                        if (cellsFromSolution > maxDeadEndLength)
                        {
                            // Find all cells > maxDeadEndLength and set to Direction.None (| Undefined?)
                            mazeBuilder.SetCell(column, row, mazeBuilder.GetDirection(column, row) & Direction.Undefined);
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
                            mazeBuilder.SetCell(column, row, entranceEdge & Direction.Undefined);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Trim a specific dead-end to the specified maximum length.
        /// </summary>
        /// <param name="mazeBuilder">The maze builder to modify.</param>
        /// <param name="metricsComputations">The metrics computations for the maze.</param>
        /// <param name="branchId">The solution path cell id.</param>
        /// <param name="maxDeadEndLength">Length in number of cells.</param>
        public static void TrimDeadEnds<N, E>(this IMazeBuilder<N, E> mazeBuilder, MazeMetricsComputations<N, E> metricsComputations, int branchId, int maxDeadEndLength)
        {
            for (int row = 0; row < mazeBuilder.Height; row++)
            {
                for (int column = 0; column < mazeBuilder.Width; column++)
                {
                    var metrics = metricsComputations.GetCellMetrics(column, row);
                    var branch = metrics.BranchId;
                    int? distance = metrics.PathDistanceToSolution;
                    if (distance.HasValue && branch.HasValue && branch.Value.solutionPathCell == branchId)
                    {
                        int cellsFromSolution = distance.Value;
                        if (cellsFromSolution > maxDeadEndLength)
                        {
                            // Find all cells > mazDeadEndLength and set to Direction.None (| Undefined?)
                            mazeBuilder.SetCell(column, row, mazeBuilder.GetDirection(column, row) & Direction.Undefined);
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
                            mazeBuilder.SetCell(column, row, entranceEdge & Direction.Undefined);
                        }
                    }
                }
            }
        }
    }
}