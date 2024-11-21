using CrawfisSoftware.Collections.Graph;

using System.Collections.Generic;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Generate a Maze using depth-first search (Recursive Backtracking)
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    public class MazeBuilderRecursiveBacktracker<N, E> : MazeBuilderAbstract<N, E>
    {
        private bool[,] visited;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">The width of the desired maze</param>
        /// <param name="height">The height of the desired maze</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        public MazeBuilderRecursiveBacktracker(int width, int height, GetGridLabel<N> nodeAccessor = null, GetEdgeLabel<E> edgeAccessor = null)
            : base(width, height, nodeAccessor, edgeAccessor)
        {
            visited = new bool[Width, Height];
        }

        /// <summary>
        /// Constructor, Takes an existing maze builder (derived from MazeBuilderAbstract) and copies the state over.
        /// </summary>
        /// <param name="mazeBuilder">A maze builder</param>
        public MazeBuilderRecursiveBacktracker(MazeBuilderAbstract<N, E> mazeBuilder) : base(mazeBuilder)
        {
            visited = new bool[Width, Height];
        }
        private void RecursiveBackTracker(bool preserveExistingCells = true)
        {
        }

        private void RecursiveBackTracker(int startingNode, bool preserveExistingCells = true)
        {
            Stack<int> currentPath = new Stack<int>();
            currentPath.Push(startingNode);
            List<int> neighbors = new List<int>(4);
            visited[startingNode % Width, startingNode / Width] = true;

            while (currentPath.Count > 0)
            {
                int currentNode = currentPath.Peek();
                neighbors.Clear();
                // Select neighbors who have not been visited.
                foreach (int neighbor in grid.Neighbors(currentNode))
                {
                    int row = neighbor / Width;
                    int column = neighbor % Width;
                    if (!visited[column, row])
                    {
                        neighbors.Add(neighbor);
                    }
                }
                bool pathCarved = false;
                int nextNode = -1;
                while (!pathCarved && neighbors.Count > 0)
                {
                    int randomNeighbor = RandomGenerator.Next(neighbors.Count);
                    nextNode = neighbors[randomNeighbor];
                    pathCarved = CarvePassage(currentNode, nextNode, preserveExistingCells);
                    visited[nextNode % Width, nextNode / Width] = true;
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

        /// <inheritdoc/>
        public override void CreateMaze(bool preserveExistingCells = true)
        {
            // Todo: Fix preserveExistingCells = false or throw an error
            RecursiveBackTracker(StartCell, preserveExistingCells);
            // Clear all Undefined flags, since maze generation should touch all cells.
            // Todo: Not true, as "grid" may be masked to certain edges.
            //RemoveUndefines();
        }
    }
}