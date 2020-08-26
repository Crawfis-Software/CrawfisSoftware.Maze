using CrawfisSoftware.Collections.Graph;
using System.Collections.Generic;
using System.Linq;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Create a maze using the Aldous Broder algorithm
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    public class MazeBuilderAldousBroder<N, E> : MazeBuilderAbstract<N, E>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">The width of the desired maze</param>
        /// <param name="height">The height of the desired maze</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        public MazeBuilderAldousBroder(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor) : base(width, height, nodeAccessor, edgeAccessor)
        {
        }
        private void AldousBroder(bool preserveExistingCells = false) // Random Walk, may take an infinite amount of time.
        {
            int unvisited = grid.NumberOfNodes - 1;
            bool[] visited = new bool[grid.NumberOfNodes];
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    int index = row * Width + column;
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

        /// <inheritdoc/>
        public override void CreateMaze(bool preserveExistingCells = false)
        {
            AldousBroder(preserveExistingCells);
        }

    }
}