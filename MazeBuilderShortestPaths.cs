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
        public int PreservedClosedValue { get; set; } = int.MaxValue - 1;

        public EdgeCostDelegate<E> EdgeFunction { get; set; }

        private int[,,] _randomValues;
        
        public MazeBuilderShortestPaths(MazeBuilderAbstract<N, E> mazeBuilder) : base(mazeBuilder)
        {
            if (this.edgeFunction == null)
                EdgeFunction = RandomEdgeCost;
        }

        public MazeBuilderShortestPaths(int width, int height, GetGridLabel<N> nodeAccessor = null, GetEdgeLabel<E> edgeAccessor = null) : base(width, height, nodeAccessor, edgeAccessor)
        {
            if (edgeAccessor == null)
                EdgeFunction = RandomEdgeCost;
        }

        public void CarveAllShortestPathsToTarget(bool preserveExistingCells = false)
        {
            if (preserveExistingCells)
            {
                _randomValues = RandomValuesWithExistingEdges();
            }
            else
            {
                _randomValues = RandomValues();
            }
            CarveShortestPaths(preserveExistingCells, TargetCell);

        }

        /// <inheritdoc/>
        public override void CreateMaze(bool preserveExistingCells = false)
        {
        }

        private void CarveShortestPaths(bool preserveExistingCells, int targetCell)
        {
            var pathQuery = new Graph.SourceShortestPaths<N, E>(grid, targetCell, EdgeFunction);
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    int targetNode = column + row * Width;
                    foreach (var cell in pathQuery.GetPath(targetNode))
                    {
                        CarvePassage(cell.From, cell.To, false);
                    }
                }
            }
        }

        private static Dictionary<Direction, int> _directionIndex = new Dictionary<Direction, int>() { { Direction.W, 0 }, { Direction.N, 1 }, {Direction.E, 2 }, {Direction.S, 3 } };
        private float RandomEdgeCost(IIndexedEdge<E> edge)
        {
            Direction direction = DirectionExtensions.GetEdgeDirection(edge.From, edge.To, Width);
            direction = direction & ~Direction.Undefined;
            int edgeValue = _randomValues[edge.From % Width, edge.From / Width, _directionIndex[direction]];
            return edgeValue;
        }

        public float EdgeComparerUsingGetEdgeLabel(IIndexedEdge<float> edge)
        {
            return edge.Value;
        }

        public float EdgeComparerUsingGetEdgeLabel(IIndexedEdge<int> edge)
        {
            return edge.Value;
        }

        private int[,,] RandomValues()
        {
            int[,,] randomValues = new int[Width, Height, 4];
            var randomGen = this.RandomGenerator;
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    for(int direction = 0; direction < 4; direction++)
                    {
                        randomValues[column, row, direction] = randomGen.Next(MinRandomValue, MaxRandomValue);
                    }
                }
            }
            return randomValues;
        }

        private int[,,] RandomValuesWithExistingEdges()
        {
            int[,,] randomValues = new int[Width, Height, 4];
            var randomGen = this.RandomGenerator;
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    bool cellsCanBeModified = true;
                    cellsCanBeModified = (directions[column, row] & Direction.Undefined) == Direction.Undefined;
                    Direction direction = this.directions[column, row];
                    if ((direction & Direction.W) == Direction.W)
                    {
                        bool useRandom = cellsCanBeModified & column != 0 && (directions[column-1, row] & Direction.Undefined) == Direction.Undefined;
                        if (useRandom) randomValues[column, row, 0] = randomGen.Next(MinRandomValue, MaxRandomValue);
                        else randomValues[column, row, 0] = PreservedOpeningValue;
                    }
                    else
                    {
                        bool useRandom = cellsCanBeModified & column != 0 && (directions[column - 1, row] & Direction.Undefined) == Direction.Undefined;
                        if (useRandom) randomValues[column, row, 0] = randomGen.Next(MinRandomValue, MaxRandomValue);
                        else randomValues[column, row, 0] = PreservedClosedValue;
                    }
                    if ((direction & Direction.N) == Direction.N)
                    {
                        bool useRandom = cellsCanBeModified & row != Height-1 && (directions[column, row+1] & Direction.Undefined) == Direction.Undefined;
                        if (useRandom) randomValues[column, row, 1] = randomGen.Next(MinRandomValue, MaxRandomValue);
                        else randomValues[column, row, 1] = PreservedOpeningValue;
                    }
                    else
                    {
                        bool useRandom = cellsCanBeModified & row != Height - 1 && (directions[column, row + 1] & Direction.Undefined) == Direction.Undefined;
                        if (useRandom) randomValues[column, row, 1] = randomGen.Next(MinRandomValue, MaxRandomValue);
                        else randomValues[column, row, 1] = PreservedClosedValue;
                    }
                    if ((direction & Direction.E) == Direction.E)
                    {
                        bool useRandom = cellsCanBeModified & column != Width - 1 && (directions[column + 1, row] & Direction.Undefined) == Direction.Undefined;
                        if (useRandom) randomValues[column, row, 2] = randomGen.Next(MinRandomValue, MaxRandomValue);
                        else randomValues[column, row, 2] = PreservedOpeningValue;
                    }
                    else
                    {
                        bool useRandom = cellsCanBeModified & column != Width - 1 && (directions[column + 1, row] & Direction.Undefined) == Direction.Undefined;
                        if (useRandom) randomValues[column, row, 2] = randomGen.Next(MinRandomValue, MaxRandomValue);
                        else randomValues[column, row, 2] = PreservedClosedValue;
                    }
                    if ((direction & Direction.S) == Direction.S)
                    {
                        bool useRandom = cellsCanBeModified & row != 0 && (directions[column, row - 1] & Direction.Undefined) == Direction.Undefined;
                        if (useRandom) randomValues[column, row, 3] = randomGen.Next(MinRandomValue, MaxRandomValue);
                        else randomValues[column, row, 3] = PreservedOpeningValue;
                    }
                    else
                    {
                        bool useRandom = cellsCanBeModified & row != 0 && (directions[column, row - 1] & Direction.Undefined) == Direction.Undefined;
                        if (useRandom) randomValues[column, row, 3] = randomGen.Next(MinRandomValue, MaxRandomValue);
                        else randomValues[column, row, 3] = PreservedClosedValue;
                    }
                }
            }
            return randomValues;
        }
    }
}