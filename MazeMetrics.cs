using CrawfisSoftware.Collections.Path;
using System;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Metrics on the overall Maze.
    /// </summary>
    public struct MazeMetrics
    {
        public GridPathMetrics<int,int> SolutionPathMetric;
        //public Nullable<int> SolutionPathLength;
        public Nullable<int> NumberOfEmptyCells;
        public Nullable<int> NumberOfDeadEndCells;
        public Nullable<int> NumberOfStraightCells;
        public Nullable<int> NumberOfTurnCells;
        public Nullable<int> NumberOfTJuntionCells;
        public Nullable<int> NumberOfCrossJunctionCells;
        public Nullable<int> NumberOfSolidCells;
        public Nullable<int> NumberOfUndefinedCells;
        public Nullable<int> MaxBranchLevel;
        public Nullable<int> MaxDeadEndLength;
        public Nullable<int> MaxDistanceFromStart;
        public Nullable<int> MaxDistanceToEnd;
        //public IList<int> SolutionPath;
    }
}