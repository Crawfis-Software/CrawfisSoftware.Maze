using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Create a maze using the Recursive Division algorithm
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    public class MazeBuilderRecursiveDivision<N, E> : MazeBuilderAbstract<N, E>
    {
        /// <summary>
        /// A function to determine whether to split horizontally or vertically.
        /// It takes in the width and height of the region and return true if it
        /// should split horizontally, false if it should split vertically. 
        /// Default implementation splits the axes with the longer side length.
        /// </summary>
        public Func<int,int,bool> SplitHorizontalOrVertical { get; set; }

        /// <summary>
        /// Function to take the current column and width and return the column to split.
        /// Should return -1 if the splitting should stop (width &lt; 2). Default implementation
        /// splits a randomly from column to column+width-1.
        /// </summary>
        public Func<int,int,int> HorizontalSplitDecision { get; set; }

        /// <summary>
        /// Function to take the current row and height and return the row to split.
        /// Should return -1 if the splitting should stop (height &lt; 2). Default implementation
        /// splits a randomly from row to row+height-1.
        /// </summary>
        public Func<int, int, int> VerticalSplitDecision { get; set; }

        /// Constructor
        /// <summary>
        /// <param name="width">The width of the desired maze</param>
        /// <param name="height">The height of the desired maze</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        /// </summary>
        public MazeBuilderRecursiveDivision(int width, int height, GetGridLabel<N> nodeAccessor = null, GetEdgeLabel<E> edgeAccessor = null)
            : base(width, height, nodeAccessor, edgeAccessor)
        {
            this.SplitHorizontalOrVertical = SplitLargestArea;
            this.HorizontalSplitDecision = SplitDecision;
            this.VerticalSplitDecision = SplitDecision;
        }

        private bool SplitLargestArea(int width, int height)
        {
            if (width < height)
                return true;
            return false;
        }
        private int SplitDecision(int index, int size)
        {
            if (size <= 1) return -1;
            return index + RandomGenerator.Next(0, size - 1);
        }

        /// <summary>
        /// Constructor, Takes an existing maze builder (derived from MazeBuilderAbstract) and copies the state over.
        /// </summary>
        /// <param name="mazeBuilder">A maze builder</param>
        public MazeBuilderRecursiveDivision(MazeBuilderAbstract<N, E> mazeBuilder) : base(mazeBuilder)
        {
        }

        private void RecursiveDivision(int column, int row, int width, int height, bool preserveExistingCells = false)
        {
            if (width > 1 && height > 1)
            {
                if (SplitHorizontalOrVertical(width,height))
                    DivideHorizontally(column, row, width, height, preserveExistingCells);
                else
                    DivideVertically(column, row, width, height, preserveExistingCells);
            }
            else if(width > 1)
            {
                DivideVertically(column, row, width, height, preserveExistingCells);
            }
            else if(height>1)
            {
                DivideHorizontally(column, row, width, height, preserveExistingCells);
            }
        }

        private void DivideHorizontally(int column, int row, int width, int height, bool preserveExistingCells)
        {
            int dividingRow = HorizontalSplitDecision(row, height);
            if (dividingRow < 0) return;
            for (int col=column; col < column+width; col++)
            {
                int currentCell = col + dividingRow * Width;
                AddWall(currentCell, currentCell+Width);
            }
            int openPassageColumn = column + RandomGenerator.Next(0, width);
            int cellIndex = openPassageColumn + dividingRow * Width;
            CarvePassage(cellIndex, cellIndex + Width);
            RecursiveDivision(column, row, width, dividingRow-row+1, preserveExistingCells);
            RecursiveDivision(column, dividingRow+1, width, row+height-dividingRow-1, preserveExistingCells);
        }

        private void DivideVertically(int column, int row, int width, int height, bool preserveExistingCells)
        {
            int dividingColumn = VerticalSplitDecision(column, width);
            if(dividingColumn < 0) return;
            for (int r = row; r < row+height; r++)
            {
                int currentCell = dividingColumn + r * Width;
                AddWall(currentCell, currentCell+1);
            }
            int openPassageRow = row + RandomGenerator.Next(0, height-1);
            int cellIndex = dividingColumn + openPassageRow * Width;
            CarvePassage(cellIndex, cellIndex + 1);
            RecursiveDivision(column, row, dividingColumn - column + 1, height, preserveExistingCells);
            RecursiveDivision(dividingColumn+1, row, column + width - dividingColumn-1, height, preserveExistingCells);
        }

        /// <inheritdoc/>
        public override void CreateMaze(bool preserveExistingCells = false)
        {
            this.OpenRegion(0, Width * Height - 1);
            WallBoundary(0, Width * Height - 1);
            RecursiveDivision(0, 0, Width, Height, preserveExistingCells);
        }

    }
}