using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Given an existing maze, utilities to carve more passages that may cause loops.
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    public class MazeBuilderWeaver<N, E> : MazeBuilderAbstract<N, E>
    {
        private MazeMetricsComputations<N, E> _mazeMetricsComputations;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mazeBuilder">A previous MazeBuilder</param>
        /// <param name="mazeMetricsComputations">(optional) Pre-computed maze metrics. If null, these will be computed if needed.</param>
        public MazeBuilderWeaver(MazeBuilderAbstract<N, E> mazeBuilder, MazeMetricsComputations<N, E> mazeMetricsComputations = null) : base(mazeBuilder)
        {
            _mazeMetricsComputations = mazeMetricsComputations;
        }

        /// <inheritdoc/>
        public override void CreateMaze(bool preserveExistingCells = false)
        {
        }

        /// <summary>
        /// Provides a braid for the maze, randomly connecting dead-end cell to a neighbor. 
        /// </summary>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.</param>
        /// <param name="carveNeighbors">True to keep the underlying maze consistent. False to just modify the dead-end cell.</param>
        public void MergeDeadEndsRandomly(bool preserveExistingCells = false, bool carveNeighbors = true)
        {
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    Direction dir = directions[column, row] & ~Direction.Undefined;
                    int incomingCellIndex = -1;
                    switch (dir)
                    {
                        case Direction.W:
                            incomingCellIndex = column - 1 + row * Width;
                            break;
                        case Direction.N:
                            incomingCellIndex = column + (row + 1) * Width;
                            break;
                        case Direction.E:
                            incomingCellIndex = column + 1 + row * Width;
                            break;
                        case Direction.S:
                            incomingCellIndex = column + (row - 1) * Width;
                            break;
                    }
                    if (incomingCellIndex != -1)
                    {
                        IList<int> neighborDirs = grid.Neighbors(column + row * Width).ToList();
                        foreach (var neighborIndex in neighborDirs.Shuffle<int>(RandomGenerator))
                        {
                            if (neighborIndex == incomingCellIndex) continue;
                            if (carveNeighbors)
                            {
                                if (CarvePassage(column + row * Width, neighborIndex, preserveExistingCells)) break;
                            }
                            else
                            {
                                // This will lead to an inconsistent edge, which is useful is certain situations.
                                var directionToCarve = DirectionExtensions.GetEdgeDirection(column + row * Width, neighborIndex, Width);
                                directions[column, row] |= directionToCarve;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void MergeCells(bool preserveExistingCells = false)
        {
            _mazeMetricsComputations ??= ComputeMetrics(preserveExistingCells);
        }

        private MazeMetricsComputations<N, E> ComputeMetrics(bool preserveExistingCells = false)
        {
            this.CreateMaze(preserveExistingCells);
            var maze = this.GetMaze();
            return new MazeMetricsComputations<N, E>(maze);
        }
    }
}