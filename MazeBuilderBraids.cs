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
    public class MazeBuilderBraids<N, E> : MazeBuilderAbstract<N, E>
    {
        private MazeMetricsComputations<N, E> _mazeMetricsComputations;
        private bool _isDirty = false;
        private Maze<N, E> _currentMaze;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mazeBuilder">A previous MazeBuilder</param>
        /// <param name="mazeMetricsComputations">(optional) Pre-computed maze metrics. If null, these will be computed if needed.</param>
        public MazeBuilderBraids(MazeBuilderAbstract<N, E> mazeBuilder, MazeMetricsComputations<N, E> mazeMetricsComputations = null) : base(mazeBuilder)
        {
            _mazeMetricsComputations = mazeMetricsComputations;
            _isDirty = _mazeMetricsComputations == null;
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
                    Direction dir = GetDirection(column, row) & ~Direction.Undefined;
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
                                AddDirectionExplicitly(column, row, directionToCarve);
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// General routine to merge adjacent cells using scores and functions.
        /// </summary>
        /// <param name="computeWallScore">A function that computes a "score" for a wall using cell metrics and edge properties.</param>
        /// <param name="thresholdToRemove">If the wall score is greater than this threshold it is added to a candidate set to carve.</param>
        /// <param name="sortResults">If true, sort the candidate set according to the score.</param>
        /// <param name="keepCarvingPredicate">A predicate to stop the carving based on the number of walls carved, the current score and the edge.</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        public void MergeAdjacentCells(Func<IIndexedEdge<E>, MazeCellMetrics, MazeCellMetrics, float> computeWallScore, float thresholdToRemove, bool sortResults, Func<int, float, IIndexedEdge<E>, bool> keepCarvingPredicate, bool preserveExistingCells = false)
        {
            if (_isDirty)
                _mazeMetricsComputations = ComputeMetrics(preserveExistingCells);
            var candidateEdges = new List<(float, IIndexedEdge<E>)>();
            foreach (var wall in grid.Edges)
            {
                var fromMetrics = _mazeMetricsComputations.GetCellMetrics(wall.From);
                var toMetrics = _mazeMetricsComputations.GetCellMetrics(wall.To);
                float score = computeWallScore(wall, fromMetrics, toMetrics);
                if (score > thresholdToRemove)
                    candidateEdges.Add((score, wall));
            }
            if (sortResults)
                candidateEdges.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            int carvedCount = 0;
            foreach (var edgeTuple in candidateEdges)
            {
                IIndexedEdge<E> edge = edgeTuple.Item2;
                Direction edgeDirection = DirectionExtensions.GetEdgeDirection(edge.From, edge.To, Width);
                if ((GetDirection(edge.From % Width, edge.From / Width) & edgeDirection) == edgeDirection) continue;
                if (!keepCarvingPredicate(carvedCount, edgeTuple.Item1, edge))
                    break;
                bool carved = CarvePassage(edge.From, edge.To, preserveExistingCells);
                if (carved) carvedCount++;
            }
            _isDirty = true;
        }

        private MazeMetricsComputations<N, E> ComputeMetrics(bool preserveExistingCells = false)
        {
            this.CreateMaze(preserveExistingCells);
            _currentMaze = this.GetMaze();
            var metrics = new MazeMetricsComputations<N, E>(_currentMaze);
            metrics.ComputeAllMetrics(RandomGenerator);
            return metrics;
        }
    }
}