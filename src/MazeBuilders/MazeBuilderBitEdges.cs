namespace CrawfisSoftware.Maze
{
    /// <summary>
    /// Create a maze given a compressed set of bit patterns for the vertical and horizontal edges on a grid
    /// </summary>
    public static class MazeBuilderBitEdges
    {
        /// <summary>
        /// Create a maze from the vertical and horizontal edge bits (legacy).
        /// </summary>
        /// <typeparam name="N">The type used for node labels</typeparam>
        /// <typeparam name="E">The type used for edge weights</typeparam>
        /// <param name="mazeBuilder">A maze builder</param>
        /// <param name="verticalBits">A bit pattern representing the vertical passages in a small maze.</param>
        /// <param name="horizontalBits">A bit pattern representing the horizontal passages in a small maze.</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined. Default is false.</param>
        public static void CarveMazeFromBitPattern<N, E>(this IMazeBuilder<N, E> mazeBuilder, int verticalBits, int horizontalBits, bool preserveExistingCells = false)
        {
            PassageBits(mazeBuilder, verticalBits, horizontalBits, preserveExistingCells);
        }

        private static void PassageBits<N, E>(IMazeBuilder<N, E> mazeBuilder, int VBP, int EBP, bool preserveExistingCells = false)
        {
            int numberOfBits = mazeBuilder.Width * (mazeBuilder.Height - 1);
            // Loop through the vertical passages adding directions. Then loop through the horizontal passages.
            VBP = ConvertVBP(mazeBuilder.Width, mazeBuilder.Height, numberOfBits, VBP);
            int[] loops = { VBP, EBP };
            int nextCellOffset = mazeBuilder.Width; // vertical
            foreach (int bitVector in loops)
            {
                for (int i = numberOfBits - 1; i >= 0; i--)
                {
                    int bitLocation = bitVector >> i;
                    bool passageExists = ((bitLocation % 2) == 1);
                    if (passageExists)
                    {
                        mazeBuilder.CarvePassage(i, i + nextCellOffset, preserveExistingCells);
                    }
                }
                nextCellOffset = 1; // horizontal
                numberOfBits = (mazeBuilder.Width - 1) * mazeBuilder.Height;
            }
        }

        private static int ConvertVBP(int width, int height, int numberOfBits, int bitVector)
        {
            // Reorder the bits to be row-wise and not column wise.
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
    }
}
