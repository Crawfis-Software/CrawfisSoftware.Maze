using OhioState.Collections.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OhioState.Collections.Maze
{
    public class MazeBuilderAldousBroder<N, E>
    {
        public MazeBuilderAldousBroder(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
        {
            this.width = width;
            this.height = height;
            nodeFunction = nodeAccessor;
            edgeFunction = edgeAccessor;
            grid = new Grid<N, E>(width, height, nodeAccessor, edgeAccessor);
            directions = new Direction[height, width];
        }
        private void AldousBroder() // Random Walk, may take an infinite amount of time.
        {
            int unvisited = grid.NumberOfNodes - 1;
            bool[] visited = new bool[grid.NumberOfNodes];
            var randomNumber = new Random();
            int randomCell = randomNumber.Next(grid.NumberOfNodes);
            visited[randomCell] = true;
            while (unvisited > 0)
            {
                List<int> neighbors = grid.Neighbors(randomCell).ToList<int>();
                //if(neighbors.Count > 0) // Actually all grid cells have at least 2 neighbors, so no need for check.
                {
                    int randomNeighbor = randomNumber.Next(neighbors.Count);
                    int selectedNeighbor = neighbors[randomNeighbor];
                    //if (directionToNeighbor != (directions[row, column] & directionToNeighbor))
                    if (!visited[selectedNeighbor])
                    {
                        visited[selectedNeighbor] = true;
                        CarvePassage(randomCell, selectedNeighbor);
                        unvisited--;
                    }
                    randomCell = selectedNeighbor;
                }
            }
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
        public Maze<N, E> GetMaze()
        {
            AldousBroder();
            directions[0, 0] |= Direction.S;
            directions[height - 1, width - 1] |= Direction.E;
            return new Maze<N, E>(grid, directions);
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