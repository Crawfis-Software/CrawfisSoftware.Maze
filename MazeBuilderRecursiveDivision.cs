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
    public class MazeBuilderRecursiveDivision<N, E>
    {
        private MazeBuilderAbstract<N, E> _mazeBuilder;
        /// <summary>
        /// A function to determine whether to split horizontally or vertically.
        /// It takes in the width and height of the region and returns true if it
        /// should split horizontally, false if it should split vertically. 
        /// Default implementation splits the axes with the longer side length.
        /// </summary>
        public Func<int, int, bool> SplitHorizontalOrVertical { get; set; }

        /// <summary>
        /// Function to take the current column and width and return the column to split.
        /// Should return -1 if the splitting should stop (width &lt; 2). Default implementation
        /// splits a randomly from column to column+width-1.
        /// </summary>
        public Func<int, int, int> HorizontalSplitDecision { get; set; }

        /// <summary>
        /// Function to take the current row and height and return the row to split.
        /// Should return -1 if the splitting should stop (height &lt; 2). Default implementation
        /// splits a randomly from row to row+height-1.
        /// </summary>
        public Func<int, int, int> VerticalSplitDecision { get; set; }

        /// <summary>
        /// Constructor, Takes an existing maze builder (derived from MazeBuilderAbstract) and copies the state over.
        /// </summary>
        /// <param name="mazeBuilder">A maze builder</param>
        public MazeBuilderRecursiveDivision(MazeBuilderAbstract<N, E> mazeBuilder)
        {
            _mazeBuilder = mazeBuilder;
            this.SplitHorizontalOrVertical = SplitLargestArea;
            this.HorizontalSplitDecision = SplitDecision;
            this.VerticalSplitDecision = SplitDecision;
        }

        /// <summary>
        /// Create a maze using the Recursive Division algorithm.
        /// </summary>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined. Default is false.</param>
        public void CreateMaze(bool preserveExistingCells = false)
        {
            _mazeBuilder.OpenRegion(0, _mazeBuilder.Width *_mazeBuilder.Height - 1, preserveExistingCells);
            _mazeBuilder.WallBoundary(preserveExistingCells);
            RecursiveDivision(0, 0, _mazeBuilder.Width, _mazeBuilder.Height, preserveExistingCells);
        }

        private void RecursiveDivision(int column, int row, int width, int height, bool preserveExistingCells = false)
        {
            if (width > 1 && height > 1)
            {
                if (SplitHorizontalOrVertical(width, height))
                    DivideHorizontally(column, row, width, height, preserveExistingCells);
                else
                    DivideVertically(column, row, width, height, preserveExistingCells);
            }
            else if (width > 1)
            {
                DivideVertically(column, row, width, height, preserveExistingCells);
            }
            else if (height > 1)
            {
                DivideHorizontally(column, row, width, height, preserveExistingCells);
            }
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
            return index + _mazeBuilder.RandomGenerator.Next(0, size - 1);
        }

        private void DivideHorizontally(int column, int row, int width, int height, bool preserveExistingCells)
        {
            int dividingRow = HorizontalSplitDecision(row, height);
            if (dividingRow < 0) return;
            for (int col = column; col < column + width; col++)
            {
                int currentCell = col + dividingRow * _mazeBuilder.Width;
                _mazeBuilder.AddWall(currentCell, currentCell + _mazeBuilder.Width, preserveExistingCells);
            }
            int openPassageColumn = column + _mazeBuilder.RandomGenerator.Next(0, width);
            int cellIndex = openPassageColumn + dividingRow * _mazeBuilder.Width;
            _mazeBuilder.CarvePassage(cellIndex, cellIndex + _mazeBuilder.Width, preserveExistingCells);
            RecursiveDivision(column, row, width, dividingRow - row + 1, preserveExistingCells);
            RecursiveDivision(column, dividingRow + 1, width, row + height - dividingRow - 1, preserveExistingCells);
        }

        private void DivideVertically(int column, int row, int width, int height, bool preserveExistingCells)
        {
            int dividingColumn = VerticalSplitDecision(column, width);
            if (dividingColumn < 0) return;
            for (int r = row; r < row + height; r++)
            {
                int currentCell = dividingColumn + r * _mazeBuilder.Width;
                _mazeBuilder.AddWall(currentCell, currentCell + 1, preserveExistingCells);
            }
            int openPassageRow = row + _mazeBuilder.RandomGenerator.Next(0, height - 1);
            int cellIndex = dividingColumn + openPassageRow * _mazeBuilder.Width;
            _mazeBuilder.CarvePassage(cellIndex, cellIndex + 1, preserveExistingCells);
            RecursiveDivision(column, row, dividingColumn - column + 1, height, preserveExistingCells);
            RecursiveDivision(dividingColumn + 1, row, column + width - dividingColumn - 1, height, preserveExistingCells);
        }
    }
}