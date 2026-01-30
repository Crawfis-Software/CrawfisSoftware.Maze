using CrawfisSoftware.Collections.Graph;
using CrawfisSoftware.Maze;

using System.Collections.Generic;
using System.Linq;

namespace CrawfisSoftware.Maze.PerfectMazes
{
    /// <summary>
    /// Extensions for IMazeBuilder for various perfect maze (spanning tree) algorithms.
    /// </summary>
    // todo: .Net Standard 3.0 and .Net 8.0 support partial static classes.
    // When Unity supports 3.0 we can make these partial and have the name w/o numbers.
    public static /*partial*/ class PerfectMazes1
    {
        /// <summary>
        /// Create a perfect maze using the Aldous Broder algorithm, which is a
        /// random walk and may take a while.
        /// </summary>
        /// <param name="mazeBuilder">A maze builder</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        /// <typeparam name="N">The type used for node labels</typeparam>
        /// <typeparam name="E">The type used for edge weights</typeparam>
        public static void AldousBroder<N, E>(this IMazeBuilder<N, E> mazeBuilder, bool preserveExistingCells = false)
        {
            int numberOfNodes = mazeBuilder.Grid.NumberOfNodes;
            int unvisited = numberOfNodes - 1;
            bool[] visited = new bool[numberOfNodes];
            for (int row = 0; row < mazeBuilder.Height; row++)
            {
                for (int column = 0; column < mazeBuilder.Width; column++)
                {
                    int index = row * mazeBuilder.Width + column;
                    Direction direction = mazeBuilder.GetDirection(column, row);
                    if ((direction & Direction.Undefined) != Direction.Undefined)
                    {
                        visited[index] = true;
                        unvisited--;
                    }
                }
            }

            int randomCell = mazeBuilder.RandomGenerator.Next(numberOfNodes);
            visited[randomCell] = true;
            while (unvisited > 0)
            {
                List<int> neighbors = mazeBuilder.Grid.Neighbors(randomCell).ToList<int>();
                //if(neighbors.Count > 0) // Actually all grid cells have at least 1 neighbor, so no need for check.
                {
                    int randomNeighbor = mazeBuilder.RandomGenerator.Next(neighbors.Count);
                    int selectedNeighbor = neighbors[randomNeighbor];
                    //if (directionToNeighbor != (directions[row, column] & directionToNeighbor))
                    if (!visited[selectedNeighbor])
                    {
                        visited[selectedNeighbor] = true;
                        mazeBuilder.CarvePassage(randomCell, selectedNeighbor, preserveExistingCells);
                        unvisited--;
                    }
                    randomCell = selectedNeighbor;
                }
            }
        }
    }
}