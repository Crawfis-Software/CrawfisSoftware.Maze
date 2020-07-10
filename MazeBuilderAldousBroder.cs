using CrawfisSoftware.Collections.Graph;
using System.Collections.Generic;
using System.Linq;

namespace CrawfisSoftware.Collections.Maze
{
    public class MazeBuilderAldousBroder<N, E> : MazeBuilderAbstract<N, E>
    {
        public MazeBuilderAldousBroder(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor) : base(width, height, nodeAccessor, edgeAccessor)
        {
        }
        private void AldousBroder(bool preserveExistingCells = false) // Random Walk, may take an infinite amount of time.
        {
            int unvisited = grid.NumberOfNodes - 1;
            bool[] visited = new bool[grid.NumberOfNodes];
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    int index = row * width + column;
                    if ((directions[column, row] & Direction.Undefined) != Direction.Undefined)
                    {
                        visited[index] = true;
                        unvisited--;
                    }
                }
            }

            int randomCell = RandomGenerator.Next(grid.NumberOfNodes);
            visited[randomCell] = true;
            while (unvisited > 0)
            {
                List<int> neighbors = grid.Neighbors(randomCell).ToList<int>();
                //if(neighbors.Count > 0) // Actually all grid cells have at least 1 neighbor, so no need for check.
                {
                    int randomNeighbor = RandomGenerator.Next(neighbors.Count);
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

        public override void CreateMaze(bool preserveExistingCells = false)
        {
            AldousBroder(preserveExistingCells);
        }

    }
}