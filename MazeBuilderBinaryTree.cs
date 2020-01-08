using CrawfisSoftware.Collections.Graph;
using System;

namespace CrawfisSoftware.Collections.Maze
{
    public class MazeBuilderBinaryTree<N> : MazeBuilderAbstract<N>
    {
        public MazeBuilderBinaryTree(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<int> edgeAccessor)
        {
            this.width = width;
            this.height = height;
            nodeFunction = nodeAccessor;
            edgeFunction = edgeAccessor;
            grid = new Grid<N, int>(width, height, nodeAccessor, edgeAccessor);
            directions = new Direction[width, height];
        }
        private void BinaryTreeMaze(int percentHorizontal = 50)
        {
            // Sidewinder
            // Initial state is that all directions are zero.
            Random random = new Random();
            const int maxRandomValue = 1000;
            if (percentHorizontal < 0) percentHorizontal = 0;
            if (percentHorizontal > 100) percentHorizontal = 100;
            int threshold = (percentHorizontal * maxRandomValue) / 100; // Favor horizontal runs
            int row, column;
            IndexedGraphEnumerator<N, int> graphWalker = new IndexedGraphEnumerator<N, int>(grid);
            foreach (var node in graphWalker.TraverseNodes())
            {
                if (grid.TryGetGridLocation(node, out column, out row))
                {
                    bool moveEast = random.Next(maxRandomValue) < threshold;
                    bool eastBorder = false;
                    if (column >= width - 1) eastBorder = true;
                    moveEast &= !eastBorder;
                    if (moveEast)
                    {
                        directions[column, row] |= Direction.E;
                        if (column + 1 <= width - 1) directions[column + 1, row] |= Direction.W;
                    }
                    else if (row < height - 1)
                    {
                        directions[column, row] |= Direction.N;
                        directions[column, row + 1] |= Direction.S;
                    }
                    else if ((row == height - 1) && !eastBorder)
                    {
                        directions[column, row] |= Direction.E;
                        if (column + 1 <= width - 1) directions[column + 1, row] |= Direction.W;
                    }
                }
            }
        }
        public override Maze<N> GetMaze()
        {
            BinaryTreeMaze(80);
            directions[0, 0] |= Direction.S;
            directions[width - 1, height - 1] |= Direction.E;
            return new Maze<N>(grid, directions);
        }
    }
}
