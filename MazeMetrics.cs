using CrawfisSoftware.Collections.Path;

using System;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Metrics on the overall Maze.
    /// </summary>
    public struct MazeMetrics
    {
        /// <summary>
        /// The solution path and metrics on it.
        /// </summary>
        public GridPathMetrics<int, int> SolutionPathMetric;
        /// <summary>
        /// Count of the number of dead-end cells.
        /// </summary>
        public Nullable<int> NumberOfDeadEndCells;
        /// <summary>
        /// Count of the number of straight cells.
        /// </summary>
        public Nullable<int> NumberOfStraightCells;
        /// <summary>
        /// Count of the number of cells with turns.
        /// </summary>
        public Nullable<int> NumberOfTurnCells;
        /// <summary>
        /// Count of the number of cells with T-junctions.
        /// </summary>
        public Nullable<int> NumberOfTJunctionCells;
        /// <summary>
        /// Count of the number of cells with cross-sections or open.
        /// </summary>
        public Nullable<int> NumberOfCrossJunctionCells;
        /// <summary>
        /// Count of cells that are not reachable.
        /// </summary>
        public Nullable<int> NumberOfSolidCells;
        /// <summary>
        /// Count of the number cells with the Undefined flag.
        /// </summary>
        public Nullable<int> NumberOfUndefinedCells;
        /// <summary>
        /// The maximum number of times a secondary or third exit will be crossed when traversing the maze.
        /// </summary>
        /// <remarks>This is dependent on how the Branch level is defined per cell and the algorithmic choice for what a branch means. Currently implementation has a main branch for each dead-end (recursively).</remarks>
        public Nullable<int> MaxBranchLevel;
        /// <summary>
        /// The maximum distance any dead-end cell is from the solution path.
        /// </summary>
        public Nullable<int> MaxDeadEndLength;
        /// <summary>
        /// The furthest distance reachable from the start node.
        /// </summary>
        public Nullable<int> MaxDistanceFromStart;
        /// <summary>
        /// The furthest distance any cell is from the exit.
        /// </summary>
        public Nullable<int> MaxDistanceToEnd;
    }
}