using OhioState.Collections.Graph;
using System;
using System.Collections.Generic;

namespace OhioState.Collections.Maze
{
    public class MazeBuilderRecursiveBacktracker<N, E>
    {
        public MazeBuilderRecursiveBacktracker(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
        {
            this.width = width;
            this.height = height;
            nodeFunction = nodeAccessor;
            edgeFunction = edgeAccessor;
            grid = new Grid<N, E>(width, height, nodeAccessor, edgeAccessor);
            directions = new Direction[height, width];
        }

        private void RecursiveBackTracker(int startingNode)
        {
            var randomNumber = new Random();
            Stack<int> currentPath = new Stack<int>();
            currentPath.Push(startingNode);
            while (currentPath.Count > 0)
            {
                int currentNode = currentPath.Peek();
                // Select neighbors who have not been visited (Direction is None).
                List<int> neighbors = new List<int>();
                foreach (int neighbor in grid.Neighbors(currentNode))
                {
                    int row = neighbor / width;
                    int column = neighbor % width;
                    if (directions[row, column] == Direction.None)
                    {
                        neighbors.Add(neighbor);
                    }
                }
                if (neighbors.Count > 0)
                {
                    int randomNeighbor = randomNumber.Next(neighbors.Count);
                    int nextNode = neighbors[randomNeighbor];
                    CarvePassage(currentNode, nextNode);
                    currentPath.Push(nextNode);
                }
                else
                {
                    currentPath.Pop();
                }

            }
        }
        public Maze<N,E> GetMaze()
        {
            RecursiveBackTracker(0);
            directions[0, 0] |= Direction.S;
            directions[height - 1, width - 1] |= Direction.E;
            return new Maze<N, E>(grid, directions);
        }
        private void CarvePassage(int currentCell, int targetCell)
        {
            int currentRow = currentCell / width;
            int currentColumn = currentCell % width;
            int selectedRow = targetCell / width;
            int selectedColumn = targetCell % width;
            Direction directionToNeighbor, directionToCurrent;
            if (grid.DirectionLookUp(currentCell, targetCell, out directionToNeighbor))
            {
                directions[currentRow, currentColumn] |= directionToNeighbor;
                if (grid.DirectionLookUp(targetCell, currentCell, out directionToCurrent))
                    directions[selectedRow, selectedColumn] |= directionToCurrent;
            }
        }

        #region Member variables
        private Grid<N, E> grid;
        private int width;
        private int height;
        private GetGridLabel<N> nodeFunction;
        private GetEdgeLabel<E> edgeFunction;
        private Direction[,] directions;
        #endregion
    }
}
