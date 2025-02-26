using CrawfisSoftware.Collections.Graph;

using System.Collections.Generic;

namespace CrawfisSoftware.Maze
{
    ///// <summary>
    ///// Generate a Maze using depth-first search (Recursive Backtracking)
    ///// </summary>
    ///// <typeparam name="N">The type used for node labels</typeparam>
    ///// <typeparam name="E">The type used for edge weights</typeparam>
    //public class MazeBuilderRecursiveBacktracker<N, E>
    //{
    //    private MazeBuilderAbstract<N, E> _mazeBuilder;
    //    private bool[,] visited;

    //    /// <summary>
    //    /// Constructor, Takes an existing maze builder (derived from MazeBuilderAbstract) and copies the state over.
    //    /// </summary>
    //    /// <param name="mazeBuilder">A maze builder</param>
    //    public MazeBuilderRecursiveBacktracker(MazeBuilderAbstract<N, E> mazeBuilder)
    //    {
    //        _mazeBuilder = mazeBuilder;
    //        visited = new bool[_mazeBuilder.Width, _mazeBuilder.Height];
    //    }
    //    /// <summary>
    //    /// Create a maze using the Recursive Backtracker algorithm.
    //    /// </summary>
    //    /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined. Default is false.</param>
    //    public void CreateMaze(bool preserveExistingCells = true)
    //    {
    //        RecursiveBackTracker(_mazeBuilder.StartCell, preserveExistingCells);
    //    }

    //    private void RecursiveBackTracker(int startingNode, bool preserveExistingCells = true)
    //    {
    //        Stack<int> currentPath = new Stack<int>();
    //        currentPath.Push(startingNode);
    //        List<int> neighbors = new List<int>(4);
    //        visited[startingNode % _mazeBuilder.Width, startingNode / _mazeBuilder.Width] = true;

    //        while (currentPath.Count > 0)
    //        {
    //            int currentNode = currentPath.Peek();
    //            neighbors.Clear();
    //            // Select neighbors who have not been visited.
    //            foreach (int neighbor in _mazeBuilder.Grid.Neighbors(currentNode))
    //            {
    //                int row = neighbor / _mazeBuilder.Width;
    //                int column = neighbor % _mazeBuilder.Width;
    //                if (!visited[column, row])
    //                {
    //                    neighbors.Add(neighbor);
    //                }
    //            }
    //            bool pathCarved = false;
    //            int nextNode = -1;
    //            while (!pathCarved && neighbors.Count > 0)
    //            {
    //                int randomNeighbor = _mazeBuilder.RandomGenerator.Next(neighbors.Count);
    //                nextNode = neighbors[randomNeighbor];
    //                pathCarved = _mazeBuilder.CarvePassage(currentNode, nextNode, preserveExistingCells);
    //                visited[nextNode % _mazeBuilder.Width, nextNode / _mazeBuilder.Width] = true;
    //                neighbors.RemoveAt(randomNeighbor);
    //            }
    //            if (pathCarved)
    //            {
    //                currentPath.Push(nextNode);
    //            }
    //            else
    //            {
    //                currentPath.Pop();
    //            }
    //        }
    //    }
    //}
}