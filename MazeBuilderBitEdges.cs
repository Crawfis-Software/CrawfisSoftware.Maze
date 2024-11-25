using CrawfisSoftware.Collections.Graph;

using System;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Create a maze given a compressed set of bit patterns for the vertical and horizontal edges on a grid
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    public class MazeBuilderBitEdges<N, E>
    {
        private MazeBuilderAbstract<N, E> _mazeBuilder;
        private int _verticalBits = 725552;
        private int _horizontalBits = 5421551;

        /// <summary>
        /// Constructor, Takes an existing maze builder (derived from MazeBuilderAbstract) and copies the state over.
        /// </summary>
        /// <param name="mazeBuilder">A maze builder</param>
        /// <param name="verticalBits">A bit pattern representing the vertical passages in a small maze.</param>
        /// <param name="horizontalBits">A bit pattern representing the horizontal passages in a small maze.</param>
        public MazeBuilderBitEdges(MazeBuilderAbstract<N, E> mazeBuilder, int verticalBits, int horizontalBits)
        {
            _mazeBuilder = mazeBuilder;
            if ((_mazeBuilder.Width > 6) || (_mazeBuilder.Height > 6))
                throw new ArgumentException("Width or Height are too large!");
            _verticalBits = verticalBits;
            _horizontalBits = horizontalBits;
        }

        /// <summary>
        /// Create a maze from the vertical and horizontal edge bits (legacy).
        /// </summary>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined. Default is false.</param>
        public void CreateMaze(bool preserveExistingCells = false)
        {
            PassageBits(_verticalBits, _horizontalBits, preserveExistingCells);
        }

        private void PassageBits(int VBP, int EBP, bool preserveExistingCells = false)
        {
            int numberOfBits = _mazeBuilder.Width * (_mazeBuilder.Height - 1);
            // Loop through the vertical passages adding directions. Then loop through the horizontal passages.
            VBP = ConvertVBP(numberOfBits, VBP);
            int[] loops = { VBP, EBP };
            int nextCellOffset = _mazeBuilder.Width; // vertical
            foreach (int bitVector in loops)
            {
                for (int i = numberOfBits - 1; i >= 0; i--)
                {
                    int bitLocation = bitVector >> i;
                    bool passageExists = ((bitLocation % 2) == 1);
                    if (passageExists)
                    {
                        _mazeBuilder.CarvePassage(i, i + nextCellOffset, preserveExistingCells);
                    }
                }
                nextCellOffset = 1; // horizontal
                numberOfBits = (_mazeBuilder.Width - 1) * _mazeBuilder.Height;
            }
        }

        private int ConvertVBP(int numberOfBits, int bitVector)
        {
            // Reorder the bits to be row-wise and not column wise.
            int newVBP = 0;
            for (int i = numberOfBits - 1; i >= 0; i--)
            {
                int row = i % (_mazeBuilder.Height - 1);
                int column = i / (_mazeBuilder.Height - 1);
                int bitLocation = bitVector >> i;
                int passageExists = bitLocation % 2;
                bitLocation = row * (_mazeBuilder.Width) + column;
                newVBP |= passageExists << bitLocation;
            }
            return newVBP;
        }
    }
}
