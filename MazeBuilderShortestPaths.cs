using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;

namespace CrawfisSoftware.Collections.Maze
{
    public class MazeBuilderShortestPaths<N, E> : MazeBuilderAbstract<N, E>
    {
        public int MinRandomValue { get; set; } = 100;
        public int MaxRandomValue { get; set; } = int.MaxValue - 1;
        public int TargetCell { get; set; }
        public int PreservedOpeningValue { get; set; } = 0;
        public int PreservedClosedvalue { get; set; } = int.MaxValue - 1;

        private int[,,] _randomValues;
        
        public MazeBuilderShortestPaths(MazeBuilderAbstract<N, E> mazeBuilder) : base(mazeBuilder)
        {
        }

        public MazeBuilderShortestPaths(int width, int height, GetGridLabel<N> nodeAccessor = null, GetEdgeLabel<E> edgeAccessor = null) : base(width, height, nodeAccessor, edgeAccessor)
        {
        }

        /// <inheritdoc/>
        public override void CreateMaze(bool preserveExistingCells = false)
        {
            _randomValues = GetRandomValues();
            CarveShortestPaths(preserveExistingCells, TargetCell);
        }

        private void CarveShortestPaths(bool preserveExistingCells, int targetCell)
        {
            var pathQuery = new Graph.SourceShortestPaths<N, E>(grid, targetCell, RandomEdgeCost);
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    int targetNode = column + row * Width;
                    foreach (var cell in pathQuery.GetPath(targetNode))
                    {
                        CarvePassage(cell.From, cell.To, preserveExistingCells);
                    }
                }
            }
        }

        private static Dictionary<Direction, int> _directionIndex = new Dictionary<Direction, int>() { { Direction.W, 0 }, { Direction.N, 1 }, {Direction.E, 2 }, {Direction.S, 3 } };
        private float RandomEdgeCost(IIndexedEdge<E> edge)
        {
            Direction direction = DirectionExtensions.GetEdgeDirection(edge.From, edge.To, Width);
            direction = direction & ~Direction.Undefined;
            int fromValue = _randomValues[edge.From % Width, edge.From / Width, _directionIndex[direction]];
            return fromValue;
        }

        private int[,,] GetRandomValues()
        {
            int[,,] randomValues = new int[Width, Height, 4];
            var randomGen = this.RandomGenerator;
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    Direction direction = this.directions[column, row];
                    if ((direction & Direction.W) == Direction.W)
                    {
                        randomValues[column, row, 0] = PreservedOpeningValue;
                    }
                    else
                    {
                        randomValues[column, row, 0] = randomGen.Next(MinRandomValue, MaxRandomValue);
                    }
                    if ((direction & Direction.N) == Direction.N)
                    {
                        randomValues[column, row, 1] = PreservedOpeningValue;
                    }
                    else
                    {
                        randomValues[column, row, 1] = randomGen.Next(MinRandomValue, MaxRandomValue);
                    }
                    if ((direction & Direction.E) == Direction.W)
                    {
                        randomValues[column, row, 2] = PreservedOpeningValue;
                    }
                    else
                    {
                        randomValues[column, row, 2] = randomGen.Next(MinRandomValue, MaxRandomValue);
                    }
                    if ((direction & Direction.S) == Direction.S)
                    {
                        randomValues[column, row, 3] = PreservedOpeningValue;
                    }
                    else
                    {
                        randomValues[column, row, 3] = randomGen.Next(MinRandomValue, MaxRandomValue);
                    }
                }
            }
            return randomValues;
        }
    }
}