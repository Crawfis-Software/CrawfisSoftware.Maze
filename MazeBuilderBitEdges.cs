using OhioState.Collections.Graph;
using System;

namespace OhioState.Collections.Maze
{
    public class MazeBuilderBitEdges<N, E>
    {
        public MazeBuilderBitEdges(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
        {
            this.width = width;
            this.height = height;
            grid = new Grid<N, E>(width, height, nodeAccessor, edgeAccessor);
            directions = new Direction[height, width];
        }

        private void PassageBits(int VBP, int EBP)
        {
            if ((width > 6) || (height > 6))
                throw new ArgumentException("Width and Height are too large!");
            int numberOfBits = width * (height - 1);
            // Loop through the vertical passages adding directions. Then loop through the horizontal passages.
            VBP = ConvertVBP(numberOfBits, VBP);
            int[] loops = { VBP, EBP };
            int nextCellOffset = width; // vertical
            foreach (int bitVector in loops)
            {
                for (int i = numberOfBits - 1; i >= 0; i--)
                {
                    int bitLocation = bitVector >> i;
                    bool passageExists = ((bitLocation % 2) == 1);
                    if (passageExists)
                    {
                        CarvePassage(i, i + nextCellOffset);
                    }
                }
                nextCellOffset = 1; // horizontal
                numberOfBits = (width - 1) * height;
            }
        }

        private int ConvertVBP(int numberOfBits, int bitVector)
        {
            // Reorder the bits to be rowwise and not column wise.
            int newVBP = 0;
            for (int i = numberOfBits - 1; i >= 0; i--)
            {
                int row = i % (height - 1);
                int column = i / (height - 1);
                int bitLocation = bitVector >> i;
                int passageExists = bitLocation % 2;
                bitLocation = row * (width) + column;
                newVBP |= passageExists << bitLocation;
            }
            return newVBP;
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
            PassageBits(725552, 5421551);
            directions[0, 0] |= Direction.S;
            directions[height - 1, width - 1] |= Direction.E;
            return new Maze<N, E>(grid, directions);
        }

        #region Member variables
        private Grid<N, E> grid;
        private readonly int width;
        private readonly int height;
        private Direction[,] directions;
        #endregion
    }
}
