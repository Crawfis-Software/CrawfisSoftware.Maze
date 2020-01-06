using OhioState.Collections.Graph;
using System;

namespace OhioState.Collections.Maze
{
    public class MazeBuilderBinaryTree<N, E>
    {
        public MazeBuilderBinaryTree(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
        {
            this.width = width;
            this.height = height;
            nodeFunction = nodeAccessor;
            edgeFunction = edgeAccessor;
            grid = new Grid<N, E>(width, height, nodeAccessor, edgeAccessor);
            directions = new Direction[height, width];
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
            IndexedGraphEnumerator<N, E> graphWalker = new IndexedGraphEnumerator<N, E>(grid);
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
                        directions[row, column] |= Direction.E;
                        if (column + 1 <= width - 1) directions[row, column + 1] |= Direction.W;
                    }
                    else if (row < height - 1)
                    {
                        directions[row, column] |= Direction.N;
                        directions[row + 1, column] |= Direction.S;
                    }
                    else if ((row == height - 1) && !eastBorder)
                    {
                        directions[row, column] |= Direction.E;
                        if (column + 1 <= width - 1) directions[row, column + 1] |= Direction.W;
                    }
                }
            }
        }
        private void CarvePassage(int currentCell, int targetCell)
        {
            int currentRow = currentCell / width;
            int currentColumn = currentCell % width;
            int selectedRow = targetCell / width;
            int selectedColumn = targetCell % width;
            Direction directionToNeighbor, directionToCurrent;
            if (grid.DirectionLookUp(currentCell, targetCell, out directionToNeighbor))
            {
                directions[currentRow, currentColumn] |= directionToNeighbor;
                if (grid.DirectionLookUp(targetCell, currentCell, out directionToCurrent))
                    directions[selectedRow, selectedColumn] |= directionToCurrent;
            }
        }
        public Maze<N, E> GetMaze()
        {
            BinaryTreeMaze(80);
            directions[0, 0] |= Direction.S;
            directions[height - 1, width - 1] |= Direction.E;
            return new Maze<N, E>(grid, directions);
        }

        #region Member variables
        private Grid<N, E> grid;
        private int width;
        private int height;
        private GetGridLabel<N> nodeFunction;
        private GetEdgeLabel<E> edgeFunction;
        private Direction[,] directions;
        #endregion
    }
}
