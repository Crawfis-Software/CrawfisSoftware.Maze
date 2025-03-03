using CrawfisSoftware.Collections.Graph;
using CrawfisSoftware.Maze;

using System;
using System.Collections.Generic;

namespace CrawfisSoftware.Maze
{
    /// <summary>
    /// Extension methods for the IMazeBuilder interface.
    /// </summary>
    public static class MazeBuilderExtensions
    {
        /// <summary>
        /// Carve a continuous horizontal passage.
        /// </summary>
        /// <param name="mazeBuilder">The IMazeBuilder to use for carving.</param>
        /// <param name="row">The row to carve</param>
        /// <param name="column1">The start (or end) of the passage.</param>
        /// <param name="column2">The end (or start) of the passage.</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        public static void CarveHorizontalSpan<N, E>(this IMazeBuilder<N, E> mazeBuilder, int row, int column1, int column2, bool preserveExistingCells)
        {
            int start = column2;
            int end = column1;
            column1 = column2;
            if (start > end)
            {
                start = end;
                end = column2;
            }
            for (int i = start; i < end; i++)
            {
                CarveDirectionally(mazeBuilder, i, row, Direction.E, preserveExistingCells);
            }
        }
        /// <summary>
        /// Carve a passage in the specified direction.
        /// </summary>
        /// <param name="mazeBuilder">The IMazeBuilder to use for carving.</param>
        /// <param name="currentColumn">Column index of the cell to carve</param>
        /// <param name="currentRow">Row index of the row to carve</param>
        /// <param name="directionToCarve">A single direction to carve</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        /// <return>Returns true if the operation was successful.</return>
        public static bool CarveDirectionally<N, E>(this IMazeBuilder<N, E> mazeBuilder, int currentColumn, int currentRow, Direction directionToCarve, bool preserveExistingCells = false)
        {
            int cellIndex = currentRow * mazeBuilder.Width + currentColumn;
            switch (directionToCarve)
            {
                case Direction.None:
                    break;
                case Direction.W:
                    return CarvePassage(mazeBuilder, cellIndex, cellIndex - 1, preserveExistingCells);
                case Direction.N:
                    return CarvePassage(mazeBuilder, cellIndex, cellIndex + mazeBuilder.Width, preserveExistingCells);
                case Direction.E:
                    return CarvePassage(mazeBuilder, cellIndex, cellIndex + 1, preserveExistingCells);
                case Direction.S:
                    return CarvePassage(mazeBuilder, cellIndex, cellIndex - mazeBuilder.Width, preserveExistingCells);
            }
            return false;
        }
        /// <summary>
        /// Add an edge from <paramref name="currentCell"/> to <paramref name="targetCell"/> and vice versa.
        /// </summary>
        /// <param name="mazeBuilder">The IMazeBuilder to use for carving.</param>
        /// <param name="currentCell">A cell index</param>
        /// <param name="targetCell">A cell index</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        /// <returns>Returns true if the operation was successful.</returns>
        public static bool CarvePassage<N, E>(this IMazeBuilder<N, E> mazeBuilder, int currentCell, int targetCell, bool preserveExistingCells = false)
        {
            int currentRow = currentCell / mazeBuilder.Width;
            int currentColumn = currentCell % mazeBuilder.Width;
            int selectedRow = targetCell / mazeBuilder.Width;
            int selectedColumn = targetCell % mazeBuilder.Width;
            return mazeBuilder.CarvePassage(currentColumn, currentRow, selectedColumn, selectedRow, preserveExistingCells);
        }

        /// <summary>
        /// Add a wall (block) in the specified direction.
        /// </summary>
        /// <param name="mazeBuilder">The IMazeBuilder to use.</param>
        /// <param name="currentColumn">Column index of the cell.</param>
        /// <param name="currentRow">Row index of the row.</param>
        /// <param name="directionToBlock">A single direction to block</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        /// <return>Returns true if the operation was successful.</return>
        public static bool BlockDirectionally<N, E>(this IMazeBuilder<N, E> mazeBuilder, int currentColumn, int currentRow, Direction directionToBlock, bool preserveExistingCells = false)
        {
            int cellIndex = currentRow * mazeBuilder.Width + currentColumn;
            switch (directionToBlock)
            {
                case Direction.None:
                    break;
                case Direction.W:
                    return AddWall(mazeBuilder, cellIndex, cellIndex - 1, preserveExistingCells);
                case Direction.N:
                    return AddWall(mazeBuilder, cellIndex, cellIndex + mazeBuilder.Width, preserveExistingCells);
                case Direction.E:
                    return AddWall(mazeBuilder, cellIndex, cellIndex + 1, preserveExistingCells);
                case Direction.S:
                    return AddWall(mazeBuilder, cellIndex, cellIndex - mazeBuilder.Width, preserveExistingCells);
            }
            return false;
        }
        /// <summary>
        /// Remove any edge from <paramref name="currentCell"/> to <paramref name="targetCell"/> and vice versa.
        /// </summary>
        /// <param name="mazeBuilder">The IMazeBuilder to use for carving.</param>
        /// <param name="currentCell">A cell index</param>
        /// <param name="targetCell">A cell index</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        /// <returns>Returns true if the operation was successful.</returns>
        public static bool AddWall<N, E>(this IMazeBuilder<N, E> mazeBuilder, int currentCell, int targetCell, bool preserveExistingCells = false)
        {
            int currentRow = currentCell / mazeBuilder.Width;
            int currentColumn = currentCell % mazeBuilder.Width;
            int selectedRow = targetCell / mazeBuilder.Width;
            int selectedColumn = targetCell % mazeBuilder.Width;
            return mazeBuilder.AddWall(currentColumn, currentRow, selectedColumn, selectedRow, preserveExistingCells);
        }
        /// <summary>
        /// Carve a continuous vertical passage.
        /// </summary>
        /// <param name="mazeBuilder">The IMazeBuilder to use for carving.</param>
        /// <param name="column">The column to carve</param>
        /// <param name="row1">The start (or end) of the passage.</param>
        /// <param name="row2">The end (or start) of the passage.</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        public static void CarveVerticalSpan<N, E>(this IMazeBuilder<N, E> mazeBuilder, int column, int row1, int row2, bool preserveExistingCells)
        {
            int start = row1;
            int end = row2;
            if (start > end)
            {
                start = end;
                end = row1;
            }
            for (int i = start; i < end; i++)
            {
                CarveDirectionally(mazeBuilder, column, i, Direction.N, preserveExistingCells);
            }
        }

        /// <summary>
        /// Invert all of the directions, keeping Undefines unchanged.
        /// </summary>
        public static void InvertDirections<N, E>(this IMazeBuilder<N, E> mazeBuilder)
        {
            Direction allDirections = Direction.None;
            //foreach (Direction dir in Enum.GetValues<Direction>()) allDirections |= dir; // Generic version not available in .Net Standard 2.1
            foreach (var dir in Enum.GetValues(typeof(Direction))) allDirections |= (Direction)dir;
            // Masks out Undefined.
            allDirections &= ~Direction.Undefined;
            for (int row = 0; row < mazeBuilder.Height; row++)
            {
                for (int column = 0; column < mazeBuilder.Width; column++)
                {
                    Direction cellDirections = mazeBuilder.GetDirection(column, row);
                    Direction isUndefined = cellDirections & Direction.Undefined;
                    Direction inverseDirection = allDirections & ~cellDirections;
                    // Add Undefine back in if it was set.
                    inverseDirection |= isUndefined;
                    mazeBuilder.SetCell(column, row, inverseDirection, false);
                }
            }
        }
        /// <summary>
        /// Delete all edges in the specified rectangle.
        /// </summary>
        /// <param name="mazeBuilder">The IMazeBuilder to use for carving.</param>
        /// <param name="lowerLeftCell">The cell index of the lower-left corner of a rectangular region</param>
        /// <param name="upperRightCell">The cell index of the upper-right corner of a rectangular region</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        public static void BlockRegion<N, E>(this IMazeBuilder<N, E> mazeBuilder, int lowerLeftCell, int upperRightCell, bool preserveExistingCells = false)
        {
            FillRegion(mazeBuilder, lowerLeftCell, upperRightCell, Direction.None, preserveExistingCells);
        }

        /// <summary>
        /// Add all edges to neighbors within the specified rectangle.
        /// </summary>
        /// <param name="mazeBuilder">The IMazeBuilder to use for carving.</param>
        /// <param name="lowerLeftCell">The cell index of the lower-left corner of a rectangular region</param>
        /// <param name="upperRightCell">The cell index of the upper-right corner of a rectangular region</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        /// <param name="markAsUndefined">If true (default), cells are also marked as Undefined (aka unfrozen).</param>
        public static void OpenRegion<N, E>(this IMazeBuilder<N, E> mazeBuilder, int lowerLeftCell, int upperRightCell, bool preserveExistingCells = false, bool markAsUndefined = true)
        {
            Direction dirs = Direction.N | Direction.E | Direction.S | Direction.W;
            if (markAsUndefined) dirs |= Direction.Undefined;
            FillRegion(mazeBuilder, lowerLeftCell, upperRightCell, dirs, preserveExistingCells);
        }

        /// <summary>
        /// Set all cells with the directions specified.
        /// </summary>
        /// <param name="mazeBuilder">The IMazeBuilder to use for carving.</param>
        /// <param name="lowerLeftCell">The lower-left corner of the region to fix.</param>
        /// <param name="upperRightCell">The upper-right corner of the region to fix.</param>
        /// <param name="dirs">List of directions to set each cell to.</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        /// <remarks>May lead to possible inconsistent neighbor directions.</remarks>
        /// <seealso>MakeBidirectionallyConsistent</seealso>
        public static void FillRegion<N, E>(this IMazeBuilder<N, E> mazeBuilder, int lowerLeftCell, int upperRightCell, Direction dirs, bool preserveExistingCells = false)
        {
            int width = mazeBuilder.Width;
            int height = mazeBuilder.Height;
            int currentRow = lowerLeftCell / width;
            int currentColumn = lowerLeftCell % width;
            int endRow = upperRightCell / width;
            int endColumn = upperRightCell % width;
            if (currentColumn < 0 || currentColumn >= width || endColumn < 0 || endColumn >= width)
            {
                throw new ArgumentOutOfRangeException("Specified cell is outside of the current maze width");
            }
            if (currentRow < 0 || currentRow >= height || endRow < 0 || endRow >= height)
            {
                throw new ArgumentOutOfRangeException("Specified cell is outside of the current maze height");
            }
            for (int row = currentRow; row <= endRow; row++)
            {
                for (int column = currentColumn; column <= endColumn; column++)
                {
                    mazeBuilder.SetCell(column, row, dirs, preserveExistingCells);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="N"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <param name="mazeBuilder">The IMazeBuilder to use for carving.</param>
        /// <param name="directions">A 2D array of Direction's to initialize the mazeBuilder from.</param>
        public static void SetDirections<N, E>(this IMazeBuilder<N, E> mazeBuilder, Direction[,] directions)
        {
            for (int i = 0; i < directions.GetLength(0); i++)
            {
                for (int j = 0; j < directions.GetLength(1); j++)
                {
                    mazeBuilder.SetCell(i, j, directions[i, j]);
                }
            }
        }


        /// <summary>
        /// Add walls (inconsistently currently) to the boundary of the define rectangle.
        /// </summary>
        /// <param name="mazeBuilder">The IMazeBuilder to use for carving.</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        public static void WallBoundary<N, E>(this IMazeBuilder<N, E> mazeBuilder, bool preserveExistingCells = false)
        {
            int width = mazeBuilder.Width;
            int lowerLeftCell = 0;
            int upperRightCell = width * mazeBuilder.Height - 1;
            int lowerRightCell = upperRightCell % width + width * (int)(lowerLeftCell / width);
            int upperLeftCell = lowerLeftCell % width + width * (int)(upperRightCell / width);
            int row = lowerLeftCell / width;
            int col;
            for (col = lowerLeftCell % width; col <= upperRightCell % width; col++)
            {
                mazeBuilder.RemoveDirections(col, row, Direction.S, preserveExistingCells);
            }
            row = upperRightCell / width;
            for (col = lowerLeftCell % width; col <= upperRightCell % width; col++)
            {
                mazeBuilder.RemoveDirections(col, row, Direction.N, preserveExistingCells);
            }
            col = lowerLeftCell % width;
            for (row = lowerLeftCell / width; row <= upperRightCell / width; row++)
            {
                mazeBuilder.RemoveDirections(col, row, Direction.W, preserveExistingCells);
            }
            col = upperRightCell % width;
            for (row = lowerLeftCell / width; row <= upperRightCell / width; row++)
            {
                mazeBuilder.RemoveDirections(col, row, Direction.E, preserveExistingCells);
            }
        }

        /// <summary>
        /// Remove dead-ends (implementation specific - one pass or many passes).
        /// Replaces dead-ends by blocking the only passage creating an empty cell
        /// </summary>
        /// <param name="mazeBuilder">The IMazeBuilder to use for carving.</param>
        /// <param name="maxCount">Maximum number of Dead-ends to trim.</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        public static int RemoveDeadEnds<N, E>(this IMazeBuilder<N, E> mazeBuilder, int maxCount = int.MaxValue, bool preserveExistingCells = false)
        {
            int count = 0;
            var deadEnds = new List<(int col1, int row1, int col2, int row2)>();
            for (int row = 0; row < mazeBuilder.Height; row++)
            {
                for (int column = 0; column < mazeBuilder.Width; column++)
                {
                    Direction dir = mazeBuilder.GetDirection(column, row) & ~Direction.Undefined;
                    switch (dir)
                    {
                        case Direction.W:
                            //AddWall(column, row, column - 1, row, preserveExistingCells);
                            deadEnds.Add((column, row, column - 1, row));
                            break;
                        case Direction.N:
                            //AddWall(column, row, column, row + 1, preserveExistingCells);
                            deadEnds.Add((column, row, column, row + 1));
                            break;
                        case Direction.E:
                            //AddWall(column, row, column + 1, row, preserveExistingCells);
                            deadEnds.Add((column, row, column + 1, row));
                            break;
                        case Direction.S:
                            //AddWall(column, row, column, row - 1, preserveExistingCells);
                            deadEnds.Add((column, row, column, row - 1));
                            break;
                    }
                }
                foreach (var deadEnd in deadEnds)
                {
                    mazeBuilder.AddWall(deadEnd.col1, deadEnd.row1, deadEnd.col2, deadEnd.row2, preserveExistingCells);
                    count++;
                    if (count >= maxCount)
                        break;
                }
            }
            return count;
        }

        /// <summary>
        /// Remove Direction.Undefined for all cells that have been defined
        /// </summary>
        /// <param name="mazeBuilder">The IMazeBuilder to use for carving.</param>
        public static void FreezeDefinedCells<N, E>(this IMazeBuilder<N, E> mazeBuilder)
        {
            for (int row = 0; row < mazeBuilder.Height; row++)
            {
                for (int column = 0; column < mazeBuilder.Width; column++)
                {
                    mazeBuilder.FreezeCellIfUndefined(row, column);
                }
            }
        }

        /// <summary>
        /// Ensures that all edges are bi-directional. In other words, a passage was not carved from A to
        /// B and not B to A.
        /// </summary>
        /// <param name="mazeBuilder">The IMazeBuilder to use for carving.</param>
        /// <param name="carvingMissingPassages">If true, fix inconsistencies by opening up both sides. If false, wall up  both sides.</param>
        /// <remarks>This will open up all inconsistencies.</remarks>
        public static void MakeBidirectionallyConsistent<N, E>(this IMazeBuilder<N, E> mazeBuilder, bool carvingMissingPassages = true)
        {
            mazeBuilder.MakeBidirectionallyConsistent(0, 0, mazeBuilder.Width - 1, mazeBuilder.Height - 1);
        }

        /// <summary>
        /// Ensures that all edges are bi-directional. In other words, a passage was not carved from A to
        /// B and not B to A.
        /// </summary>
        /// <param name="mazeBuilder">The IMazeBuilder to use for carving.</param>
        /// <param name="lowerLeftCell">The lower-left corner of the region to fix.</param>
        /// <param name="upperRightCell">The upper-right corner of the region to fix.</param>
        /// <param name="carvingMissingPassages">If true, fix inconsistencies by opening up both sides. If false, wall up  both sides.</param>
        /// <remarks>This will open up all inconsistencies.</remarks>
        public static void MakeBidirectionallyConsistent<N, E>(this IMazeBuilder<N, E> mazeBuilder, int lowerLeftCell, int upperRightCell, bool carvingMissingPassages = true)
        {
            int width = mazeBuilder.Width;
            int currentRow = lowerLeftCell / width;
            int currentColumn = lowerLeftCell % width;
            int endRow = upperRightCell / width;
            int endColumn = upperRightCell % width;
            mazeBuilder.MakeBidirectionallyConsistent(currentColumn, currentRow, endColumn, endRow);
        }

    }
}