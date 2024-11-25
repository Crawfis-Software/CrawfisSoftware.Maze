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
    public class MazeBuilderAldousBroder<N, E>
    {
        private MazeBuilderAbstract<N, E> _mazeBuilder;

        /// <summary>
        /// Constructor, Takes an existing maze builder (derived from MazeBuilderAbstract) and copies the state over.
        /// </summary>
        /// <param name="mazeBuilder">A maze builder</param>
        public MazeBuilderAldousBroder(MazeBuilderAbstract<N, E> mazeBuilder)
        {
            _mazeBuilder = mazeBuilder;
        }

        /// <summary>
        /// Create a maze using the Aldous Broder algorithm
        /// </summary>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        public void CreateMaze(bool preserveExistingCells = false)
        {
            AldousBroder(preserveExistingCells);
        }

        private void AldousBroder(bool preserveExistingCells = false) // Random Walk, may take an infinite amount of time.
        {
            int numberOfNodes = _mazeBuilder.Grid.NumberOfNodes;
            int unvisited = numberOfNodes - 1;
            bool[] visited = new bool[numberOfNodes];
            for (int row = 0; row < _mazeBuilder.Height; row++)
            {
                for (int column = 0; column < _mazeBuilder.Width; column++)
                {
                    int index = row * _mazeBuilder.Width + column;
                    Direction direction = _mazeBuilder.GetDirection(column, row);
                    if ((direction & Direction.Undefined) != Direction.Undefined)
                    {
                        visited[index] = true;
                        unvisited--;
                    }
                }
            }

            int randomCell = _mazeBuilder.RandomGenerator.Next(numberOfNodes);
            visited[randomCell] = true;
            while (unvisited > 0)
            {
                List<int> neighbors = _mazeBuilder.Grid.Neighbors(randomCell).ToList<int>();
                //if(neighbors.Count > 0) // Actually all grid cells have at least 1 neighbor, so no need for check.
                {
                    int randomNeighbor = _mazeBuilder.RandomGenerator.Next(neighbors.Count);
                    int selectedNeighbor = neighbors[randomNeighbor];
                    //if (directionToNeighbor != (directions[row, column] & directionToNeighbor))
                    if (!visited[selectedNeighbor])
                    {
                        visited[selectedNeighbor] = true;
                        _mazeBuilder.CarvePassage(randomCell, selectedNeighbor, preserveExistingCells);
                        unvisited--;
                    }
                    randomCell = selectedNeighbor;
                }
            }
        }
    }
}