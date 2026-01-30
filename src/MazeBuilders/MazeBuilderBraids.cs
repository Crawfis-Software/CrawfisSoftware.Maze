using CrawfisSoftware.Collections.Graph;
using CrawfisSoftware.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace CrawfisSoftware.Maze
{
    /// <summary>
    /// Given an existing maze builder, extensions to carve more passages that may cause loops or braid a perfect maze.
    /// </summary>
    public static class MazeBuilderBraids
    {
        /// <summary>
        /// Provides a braid for the maze, randomly connecting dead-end cell to a neighbor. 
        /// </summary>
        /// <typeparam name="N">The type used for node labels</typeparam>
        /// <typeparam name="E">The type used for edge weights</typeparam>
        /// <param name="mazeBuilder">the underlying MazeBuilder</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.</param>
        /// <param name="carveNeighbors">True to keep the underlying maze consistent. False to just modify the dead-end cell.</param>
        public static void MergeDeadEndsRandomly<N, E>(this IMazeBuilder<N, E> mazeBuilder, bool preserveExistingCells = false, bool carveNeighbors = true)
        {
            int Width = mazeBuilder.Width;
            for (int row = 0; row < mazeBuilder.Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    Direction dir = mazeBuilder.GetDirection(column, row) & ~Direction.Undefined;
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
                        TryCarveRandomDirection(mazeBuilder, row, column, preserveExistingCells, carveNeighbors);
                    }
                }
            }
        }
        /// <summary>
        /// Provides a braid for the maze, randomly connecting dead-end cell to a neighbor. 
        /// </summary>
        /// <typeparam name="N">The type used for node labels</typeparam>
        /// <typeparam name="E">The type used for edge weights</typeparam>
        /// <param name="mazeBuilder">the underlying MazeBuilder</param>
        /// <param name="numberToMerge">The number of dead ends to try to merge.</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.</param>
        /// <param name="carveNeighbors">True to keep the underlying maze consistent. False to just modify the dead-end cell.</param>
        public static void MergeRandomDeadEnds<N, E>(this IMazeBuilder<N, E> mazeBuilder, int numberToMerge, bool preserveExistingCells = false, bool carveNeighbors = true)
        {
            int Width = mazeBuilder.Width;
            foreach (var deadEnd in mazeBuilder.GetMaze().DeadEnds().ToList().Shuffle(mazeBuilder.RandomGenerator))
            {
                if (numberToMerge-- <= 0) break;
                TryCarveRandomDirection(mazeBuilder, deadEnd.Row, deadEnd.Column, preserveExistingCells, carveNeighbors);
            }
        }

        public static void TryCarveRandomDirection<N, E>(IMazeBuilder<N, E> mazeBuilder, int row, int column, bool preserveExistingCells = false, bool carveNeighbors = true)
        {
            int width = mazeBuilder.Width;
            IList<int> neighborDirs = mazeBuilder.Grid.Neighbors(column + row * width).ToList();
            Direction direction = mazeBuilder.GetDirection(column, row);
            int index = column + width * row;
            foreach (var neighborIndex in neighborDirs.Shuffle<int>(mazeBuilder.RandomGenerator))
            {
                Direction neighborDirection = DirectionExtensions.GetEdgeDirection(index, neighborIndex, width);
                if ((direction & neighborDirection) == neighborDirection) continue; // Direction already exists.
                if (carveNeighbors)
                {
                    if (mazeBuilder.CarvePassage(column + row * width, neighborIndex, preserveExistingCells)) break;
                }
                else
                {
                    // This will lead to an inconsistent edge, which is useful is certain situations.
                    var directionToCarve = DirectionExtensions.GetEdgeDirection(column + row * width, neighborIndex, width);
                    mazeBuilder.AddDirectionExplicitly(column, row, directionToCarve);
                    break;
                }
            }
        }

        /// <summary>
        /// General routine to merge adjacent cells using scores and functions.
        /// </summary>
        /// <typeparam name="N">The type used for node labels</typeparam>
        /// <typeparam name="E">The type used for edge weights</typeparam>
        /// <param name="mazeBuilder">the underlying MazeBuilder</param>
        /// <param name="mazeMetricsComputations">Pre-computed maze metrics.</param>
        /// <param name="computeWallScore">A function that computes a "score" for a wall using cell metrics and edge properties.</param>
        /// <param name="thresholdToRemove">If the wall score is greater than this threshold it is added to a candidate set to carve.</param>
        /// <param name="sortResults">If true, sort the candidate set according to the score.</param>
        /// <param name="keepCarvingPredicate">A predicate to stop the carving based on the number of walls carved, the current score and the edge.</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        public static void MergeAdjacentCells<N, E>(this IMazeBuilder<N, E> mazeBuilder, MazeMetricsComputations<N, E> mazeMetricsComputations,
            Func<IIndexedEdge<E>, MazeCellMetrics, MazeCellMetrics, float> computeWallScore,
            float thresholdToRemove, bool sortResults,
            Func<int, float, IIndexedEdge<E>, bool> keepCarvingPredicate, bool preserveExistingCells = false)
        {
            var candidateEdges = new List<(float, IIndexedEdge<E>)>();
            foreach (var wall in mazeBuilder.Grid.Edges)
            {
                var fromMetrics = mazeMetricsComputations.GetCellMetrics(wall.From);
                var toMetrics = mazeMetricsComputations.GetCellMetrics(wall.To);
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
                Direction edgeDirection = DirectionExtensions.GetEdgeDirection(edge.From, edge.To, mazeBuilder.Width);
                if ((mazeBuilder.GetDirection(edge.From % mazeBuilder.Width, edge.From / mazeBuilder.Width) & edgeDirection) == edgeDirection) continue;
                if (!keepCarvingPredicate(carvedCount, edgeTuple.Item1, edge))
                    break;
                bool carved = mazeBuilder.CarvePassage(edge.From, edge.To, preserveExistingCells);
                if (carved) carvedCount++;
            }
        }
    }
}