using CrawfisSoftware.Collections.Maze;

using System.Collections.Generic;

namespace CrawfisSoftware.Maze.PerfectMazes
{
    /// <summary>
    /// Extensions for IMazeBuilder for various perfect maze (spanning tree) algorithms.
    /// </summary>
    // todo: .Net Standard 3.0 and .Net 8.0 support partial static classes.
    // When Unity supports 3.0 we can make these partial and have the name w/o numbers.
    public static /*partial*/ class PerfectMazes
    {
        /// <summary>
        /// Generate a Maze using depth-first search (Recursive Backtracking)
        /// </summary>
        /// <param name="mazeBuilder">A maze builder</param>
        /// <param name="startingNode">An optional node to start from. Only reachable nodes will be carved.</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined. Default is false.</param>
        /// <typeparam name="N">The type used for node labels</typeparam>
        /// <typeparam name="E">The type used for edge weights</typeparam>
        /// <remarks>If some cells are preserved, then this algorithm will not cross that boundary.</remarks>
        public static void RecursiveBacktracking<N, E>(this IMazeBuilder<N, E> mazeBuilder, int startingNode = 0, bool preserveExistingCells = false)
        {
            bool[,] visited = new bool[mazeBuilder.Width, mazeBuilder.Height];
            Stack<int> currentPath = new Stack<int>();
            currentPath.Push(startingNode);
            List<int> neighbors = new List<int>(4);
            visited[startingNode % mazeBuilder.Width, startingNode / mazeBuilder.Width] = true;

            while (currentPath.Count > 0)
            {
                int currentNode = currentPath.Peek();
                neighbors.Clear();
                // Select neighbors who have not been visited.
                foreach (int neighbor in mazeBuilder.Grid.Neighbors(currentNode))
                {
                    int row = neighbor / mazeBuilder.Width;
                    int column = neighbor % mazeBuilder.Width;
                    if (!visited[column, row])
                    {
                        neighbors.Add(neighbor);
                    }
                }
                bool pathCarved = false;
                int nextNode = -1;
                while (!pathCarved && neighbors.Count > 0)
                {
                    int randomNeighbor = mazeBuilder.RandomGenerator.Next(neighbors.Count);
                    nextNode = neighbors[randomNeighbor];
                    pathCarved = mazeBuilder.CarvePassage(currentNode, nextNode, preserveExistingCells);
                    visited[nextNode % mazeBuilder.Width, nextNode / mazeBuilder.Width] = true;
                    neighbors.RemoveAt(randomNeighbor);
                }
                if (pathCarved)
                {
                    currentPath.Push(nextNode);
                }
                else
                {
                    currentPath.Pop();
                }
            }
        }
    }
}