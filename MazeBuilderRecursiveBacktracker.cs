using CrawfisSoftware.Collections.Graph;
using System.Collections.Generic;

namespace CrawfisSoftware.Collections.Maze
{
    public class MazeBuilderRecursiveBacktracker<N, E> : MazeBuilderAbstract<N, E>
    {
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
                    int row = neighbor / width;
                    int column = neighbor % width;
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

        public override void CreateMaze(bool preserveExistingCells = true)
        {
            // Todo: Fix preserveExistingCells = false or throw an error
            RecursiveBackTracker(StartCell);
        }
    }
}
