using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;

namespace CrawfisSoftware.Collections.Maze
{
    public class MazeBuilderRecursiveBacktracker<N, E> : MazeBuilderAbstract<N,E>
    {
        public MazeBuilderRecursiveBacktracker(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor) : base(width, height, nodeAccessor, edgeAccessor)
        {
        }

        private void RecursiveBackTracker(int startingNode, bool preserveExistingCells = true)
        {
            Stack<int> currentPath = new Stack<int>();
            currentPath.Push(startingNode);
            while (currentPath.Count > 0)
            {
                int currentNode = currentPath.Peek();
                // Select neighbors who have not been visited (Direction is None).
                List<int> neighbors = new List<int>();
                foreach (int neighbor in grid.Neighbors(currentNode))
                {
                    int row = neighbor / width;
                    int column = neighbor % width;
                    if (directions[column, row] == Direction.Undefined)
                    {
                        neighbors.Add(neighbor);
                    }
                }
                if (neighbors.Count > 0)
                {
                    int randomNeighbor = RandomGenerator.Next(neighbors.Count);
                    int nextNode = neighbors[randomNeighbor];
                    CarvePassage(currentNode, nextNode);
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
