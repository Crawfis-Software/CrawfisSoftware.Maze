using CrawfisSoftware.Collections.Graph;
using System.Collections.Generic;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    public class MazeBuilderRecursiveBacktracker<N, E> : MazeBuilderAbstract<N, E>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">The width of the desired maze</param>
        /// <param name="height">The height of the desired maze</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        public MazeBuilderRecursiveBacktracker(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor) : base(width, height, nodeAccessor, edgeAccessor)
        {
        }

        private void RecursiveBackTracker(int startingNode, bool preserveExistingCells = true)
        {
            Stack<int> currentPath = new Stack<int>();
            currentPath.Push(startingNode);
            List<int> neighbors = new List<int>(4);

            while (currentPath.Count > 0)
            {
                int currentNode = currentPath.Peek();
                neighbors.Clear();
                // Select neighbors who have not been visited (Direction is None).
                foreach (int neighbor in grid.Neighbors(currentNode))
                {
                    int row = neighbor / Width;
                    int column = neighbor % Width;
                    if (directions[column, row] == Direction.Undefined)
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
                    // Holy crap!!! The next line changes the value of nextNode. I had it above pathCarved.
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
            RecursiveBackTracker(StartCell);
        }
    }
}
