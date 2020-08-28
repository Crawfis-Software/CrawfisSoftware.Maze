using CrawfisSoftware.Collections.Graph;
using System;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Create a maze given a compressed set of bit patterns for the vertical and horizontal edges on a grid
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    public class MazeBuilderBitEdges<N, E> : MazeBuilderAbstract<N, E>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">The width of the desired maze</param>
        /// <param name="height">The height of the desired maze</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        public MazeBuilderBitEdges(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor) : base(width, height, nodeAccessor, edgeAccessor)
        {
        }

        /// <summary>
        /// Constructor, Takes an existing maze builder (derived from MazeBuilderAbstract) and copies the state over.
        /// </summary>
        /// <param name="mazeBuilder">A maze builder</param>
        public MazeBuilderBitEdges(MazeBuilderAbstract<N, E> mazeBuilder) : base(mazeBuilder)
        {
        }

        private void PassageBits(int VBP, int EBP)
        {
            if ((Width > 6) || (Height > 6))
                throw new ArgumentException("Width and Height are too large!");
            int numberOfBits = Width * (Height - 1);
            // Loop through the vertical passages adding directions. Then loop through the horizontal passages.
            VBP = ConvertVBP(numberOfBits, VBP);
            int[] loops = { VBP, EBP };
            int nextCellOffset = Width; // vertical
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
                numberOfBits = (Width - 1) * Height;
            }
        }

        private int ConvertVBP(int numberOfBits, int bitVector)
        {
            // Reorder the bits to be rowwise and not column wise.
            int newVBP = 0;
            for (int i = numberOfBits - 1; i >= 0; i--)
            {
                int row = i % (Height - 1);
                int column = i / (Height - 1);
                int bitLocation = bitVector >> i;
                int passageExists = bitLocation % 2;
                bitLocation = row * (Width) + column;
                newVBP |= passageExists << bitLocation;
            }
            return newVBP;
        }

        /// <inheritdoc/>
        public override void CreateMaze(bool preserveExistingCells = false)
        {
            PassageBits(725552, 5421551);
        }
    }
}
