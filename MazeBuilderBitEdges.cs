using CrawfisSoftware.Collections.Graph;
using System;

namespace CrawfisSoftware.Collections.Maze
{
    public class MazeBuilderBitEdges<N, E> : MazeBuilderAbstract<N, E>
    {
        public MazeBuilderBitEdges(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor) : base(width, height, nodeAccessor, edgeAccessor)
        {
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

        public override void CreateMaze(bool preserveExistingCells = false)
        {
            PassageBits(725552, 5421551);
        }
    }
}
