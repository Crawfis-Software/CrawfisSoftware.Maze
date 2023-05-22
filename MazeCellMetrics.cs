using CrawfisSoftware.Collections.Graph;
using System;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Encoding for maze's that can prescribe the "direction" from the start (Entrance to an exit). Alternative exits from main paths.
    /// </summary>
    public enum EdgeFlow {
        /// <summary>
        /// The node is unreachable.
        /// </summary>
        None,
        /// <summary>
        /// THe entrance to the cell.
        /// </summary>
        Entrance,
        /// <summary>
        /// An exit from the cell. Typically along the solution path or main dead-end branch.
        /// </summary>
        PrimaryExit, 
        /// <summary>
        /// There is more than 1 exit and this one is labeled as secondary.
        /// </summary>
        SecondaryExit, 
        /// <summary>
        /// Typically, the cell is a cross section with one exit labeled as Primary another as Secondary and this one as a 3rd possible exit.
        /// </summary>
        ThirdExit
    };

    /// <summary>
    /// Metrics on a per cell basis for a maze.
    /// </summary>
    public struct MazeCellMetrics
    {
        /// <summary>
        /// The distance travelled from the start to reach this cell.
        /// </summary>
        public Nullable<int> DistanceFromStart;
        /// <summary>
        /// Distance needed to reach the exit.
        /// </summary>
        public Nullable<int> DistanceToEnd;
        /// <summary>
        /// A label for the left edge of the cell.
        /// </summary>
        public Nullable<EdgeFlow> LeftEdgeFlow;
        /// <summary>
        /// A label for the top edge of the cell.
        /// </summary>
        public Nullable<EdgeFlow> TopEdgeFlow;
        /// <summary>
        /// A label for the right edge of the cell.
        /// </summary>
        public Nullable<EdgeFlow> RightEdgeFlow;
        /// <summary>
        /// A label for the bottom edge of the cell.
        /// </summary>
        public Nullable<EdgeFlow> BottomEdgeFlow;
        /// <summary>
        /// An indicator of how deep in the maze (from a complexity point of view) or the number of decisions. It depends on the algorithm used to calculate the branch levels.
        /// </summary>
        public Nullable<int> BranchLevel;
        /// <summary>
        /// The distance needed to get back to the solution path.
        /// </summary>
        public Nullable<int> PathDistanceToSolution;
        /// <summary>
        /// How close to the solution path this cell is ignoring the maze tunnels (as a bird flies)
        /// </summary>
        public Nullable<int> GridDistanceToSolution;
        /// <summary>
        /// A unique id for the entire dead-end branch. It contains the solution cell that the branch is attached to and the edge it is attached to.
        /// </summary>
        public Nullable<(int solutionPathCell, Direction edge)> BranchId;
        /// <summary>
        /// The node that was used to reach this node.
        /// </summary>
        public Nullable<int> Parent;
    }
}