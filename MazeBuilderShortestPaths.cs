using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;

namespace CrawfisSoftware.Collections.Maze
{
    public class MazeBuilderShortestPaths<N, E> : MazeBuilderAbstract<N, E>
    {

        public static float EdgeComparerUsingGetEdgeLabel(IIndexedEdge<float> edge, Direction fromCell, Direction toCell)
        {
            return edge.Value;
        }

        public static float ConstantOfOne(IIndexedEdge<E> edge, Direction fromCell, Direction toCell)
        {
            return 1;
        }

        public static float EdgeComparerUsingGetEdgeLabel(IIndexedEdge<int> edge, Direction fromCell, Direction toCell)
        {
            return edge.Value;
        }

        public Func<IIndexedEdge<E>, Direction, Direction, float> EdgeFunction { get; set; }

        private int[,,] _randomValues;
        
        public MazeBuilderShortestPaths(MazeBuilderAbstract<N, E> mazeBuilder) : base(mazeBuilder)
        {
                EdgeFunction = ConstantOfOne;
        }

        public MazeBuilderShortestPaths(int width, int height, GetGridLabel<N> nodeAccessor = null, GetEdgeLabel<E> edgeAccessor = null) : base(width, height, nodeAccessor, edgeAccessor)
        {
                EdgeFunction = ConstantOfOne;
        }

        public void CarvePath(int startingCell, int endingCell, bool preserveExistingCells = false)
        {
            //if (preserveExistingCells)
            //{
            //    _randomValues = RandomValuesWithExistingEdges();
            //}
            //else
            //{
            //    _randomValues = RandomValues();
            //}
            foreach (var cell in PathQuery<N, E>.FindPath(grid, startingCell, endingCell, EdgeComparerUsingGetEdgeLabel))
            {
                CarvePassage(cell.From, cell.To, false);
            }
        }

        public void CarveAllShortestPathsToTarget(int targetCell, bool preserveExistingCells = false, float maxCost = float.MaxValue)
        {
            //if (preserveExistingCells)
            //{
            //    _randomValues = RandomValuesWithExistingEdges();
            //}
            //else
            //{
            //    _randomValues = RandomValues();
            //}
            CarveShortestPaths(preserveExistingCells, targetCell, maxCost);

        }

        /// <inheritdoc/>
        public override void CreateMaze(bool preserveExistingCells = false)
        {
        }

        private void CarveShortestPaths(bool preserveExistingCells, int targetCell, float maxCost = float.MaxValue)
        {
            var pathQuery = new Graph.SourceShortestPaths<N, E>(grid, targetCell, EdgeComparerUsingGetEdgeLabel);
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    int targetNode = column + row * Width;
                    if (pathQuery.GetCost(targetNode) >= maxCost) continue;
                    foreach (var cell in pathQuery.GetPath(targetNode))
                    {
                        CarvePassage(cell.From, cell.To, false);
                    }
                }
            }
        }

        public float EdgeComparerUsingGetEdgeLabel(IIndexedEdge<E> edge)
        {
            return EdgeFunction(edge, directions[edge.From % Width, edge.From / Width], directions[edge.To % Width, edge.To / Width]);
        }
    }
}