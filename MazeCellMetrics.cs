using CrawfisSoftware.Collections.Graph;
using System;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Encoding for maze's that can prescribe the "direction" from the start (Entrance to an exit). Alternative exits from main paths.
    /// </summary>
    public enum EdgeFlow { None, Entrance, PrimaryExit, SecondaryExit, ThirdExit };

    public struct MazeCellMetrics
    {
        public Nullable<int> DistanceFromStart;
        public Nullable<int> DistanceToEnd;
        public Nullable<EdgeFlow> LeftEdgeFlow;
        public Nullable<EdgeFlow> TopEdgeFlow;
        public Nullable<EdgeFlow> RightEdgeFlow;
        public Nullable<EdgeFlow> BottomEdgeFlow;
        public Nullable<int> BranchLevel;
        public Nullable<int> PathDistanceToSolution;
        public Nullable<int> GridDistanceToSolution;
        public Nullable<(int solutionPathCell, Direction edge)> BranchId;
        public Nullable<int> Parent;
    }
}