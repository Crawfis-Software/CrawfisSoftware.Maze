using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;

namespace CrawfisSoftware.Collections.Maze
{
    public class MazeBuilderRecursiveBacktracker<N, E> : MazeBuilderAbstract<N,E>
    {
        public MazeBuilderRecursiveBacktracker(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
        {
            this.width = width;
            this.height = height;
            nodeFunction = nodeAccessor;
            edgeFunction = edgeAccessor;
            grid = new Grid<N, E>(width, height, nodeAccessor, edgeAccessor);
            directions = new Direction[width, height];
        }

        private void RecursiveBackTracker(int startingNode)
        {
            var randomNumber = new Random();
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
                    if (directions[column, row] == Direction.None)
                    {
                        neighbors.Add(neighbor);
                    }
                }
                if (neighbors.Count > 0)
                {
                    int randomNeighbor = randomNumber.Next(neighbors.Count);
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
        public override Maze<N,E> GetMaze()
        {
            RecursiveBackTracker(10);
            directions[0, 0] |= Direction.S;
            directions[width - 1, height - 1] |= Direction.E;
            return new Maze<N, E>(grid, directions);
        }
    }
}
