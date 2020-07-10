using CrawfisSoftware.Collections.Graph;

namespace CrawfisSoftware.Collections.Maze
{
    public class MazeBuilderBinaryTree<N, E> : MazeBuilderAbstract<N, E>
    {
        public MazeBuilderBinaryTree(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor) : base(width, height, nodeAccessor, edgeAccessor)
        {
        }
        private void BinaryTreeMaze(int percentHorizontal = 50)
        {
            // Sidewinder
            // Initial state is that all directions are zero.
            const int maxRandomValue = 1000;
            if (percentHorizontal < 0) percentHorizontal = 0;
            if (percentHorizontal > 100) percentHorizontal = 100;
            int threshold = (percentHorizontal * maxRandomValue) / 100; // Favor horizontal runs
            int row, column;
            IndexedGraphEnumerator<N, E> graphWalker = new IndexedGraphEnumerator<N, E>(grid);
            foreach (var node in graphWalker.TraverseNodes())
            {
                if (grid.TryGetGridLocation(node, out column, out row))
                {
                    bool moveEast = RandomGenerator.Next(maxRandomValue) < threshold;
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

        public override void CreateMaze(bool preserveExistingCells = false)
        {
            // Todo: Throw an exception if preserveExistingCells = true;
            BinaryTreeMaze();
        }
    }
}
