using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrawfisSoftware.Collections.Maze
{
    public class MazeBuilderAldousBroder<N, E> : MazeBuilderAbstract<N, E>
    {
        public MazeBuilderAldousBroder(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
        {
            this.width = width;
            this.height = height;
            nodeFunction = nodeAccessor;
            edgeFunction = edgeAccessor;
            grid = new Grid<N, E>(width, height, nodeAccessor, edgeAccessor);
            directions = new Direction[width, height];
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

        public override Maze<N, E> GetMaze()
        {
            AldousBroder();
            directions[0, 0] |= Direction.S;
            directions[width - 1, height - 1] |= Direction.E;
            return new Maze<N, E>(grid, directions);
        }
    }
}