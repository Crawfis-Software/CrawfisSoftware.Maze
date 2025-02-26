using CrawfisSoftware.Collections.Graph;
using CrawfisSoftware.Maze;

namespace CrawfisSoftware.Maze
{
    /// <summary>
    /// Extension methods to expand a given Maze Builder.
    /// </summary>
    public static class MazeBuilderExpander
    {
        /// <summary>
        /// Expand an existing MazeBuilder to a new one having wider openings, walls, and/or borders
        /// </summary>
        /// <typeparam name="N">The type used for node labels</typeparam>
        /// <typeparam name="E">The type used for edge weights</typeparam>
        /// <param name="mazeBuilder"></param>
        /// <param name="numberOfOpeningTiles">The number of cells to expand each opening to.</param>
        /// <param name="numberOfWallTiles">The number of cells to expand each wall to.</param>
        /// <param name="numberOfBorderTiles">The number of cells for the border.</param>
        /// <returns>A new IMazeBuilder.</returns>
        /// <remarks>Note: The Start and End cells will be set to the interior of the maze corresponding to the mapped cell location previously. 
        /// Use one of the path carving algorithms to create an exit out of the boundary.</remarks>
        public static void ExpandMaze<N, E>(this IMazeBuilder<N, E> mazeBuilder, int numberOfOpeningTiles, int numberOfWallTiles, int numberOfBorderTiles)
        {
            int width = mazeBuilder.Width;
            int height = mazeBuilder.Height;
            int startCell = mazeBuilder.StartCell;
            int endCell = mazeBuilder.EndCell;
            int startColumn = startCell % width;
            int startRow = startCell / width;
            int endColumn = endCell % width;
            int endRow = endCell / width;

            startCell = numberOfBorderTiles + startColumn * (numberOfOpeningTiles + numberOfWallTiles) + numberOfOpeningTiles / 2
                + numberOfBorderTiles + width * startRow * (numberOfOpeningTiles + numberOfWallTiles) + width * numberOfOpeningTiles / 2;
            endCell = numberOfBorderTiles + endColumn * (numberOfOpeningTiles + numberOfWallTiles) + numberOfOpeningTiles / 2
                + numberOfBorderTiles + width * endRow * (numberOfOpeningTiles + numberOfWallTiles) + width * numberOfOpeningTiles / 2;
            width = width + width * numberOfOpeningTiles + width * numberOfWallTiles + 2 * numberOfBorderTiles;
            height = height + height * numberOfOpeningTiles + height * numberOfWallTiles + 2 * numberOfBorderTiles;
            var newDirections = ExpandDirections(mazeBuilder, width, height, numberOfOpeningTiles, numberOfWallTiles, numberOfBorderTiles);
            var expandedMazeBuilder = new MazeBuilder<N, E>(width, height);
            expandedMazeBuilder.SetDirections(newDirections);
        }

        private static Direction[,] ExpandDirections<N, E>(IMazeBuilder<N, E> mazeBuilder, int width, int height, int _numberOfOpeningTiles, int _numberOfWallTiles, int _numberOfBorderTiles)
        {
            var newDirections = new Direction[width, height];
            for (int row = _numberOfBorderTiles; row < height - _numberOfBorderTiles; row++)
            {
                int expansionSize = (1 + _numberOfOpeningTiles + _numberOfWallTiles);
                int oldRow = (row - _numberOfBorderTiles) / expansionSize;
                for (int column = _numberOfBorderTiles; column < width - _numberOfBorderTiles; column++)
                {
                    newDirections[column, row] = Direction.None;
                    int oldColumn = (column - _numberOfBorderTiles) / expansionSize;
                    Direction direction = mazeBuilder.GetDirection(oldColumn, oldRow);
                    if ((direction | Direction.Undefined) == (Direction.None | Direction.Undefined)) continue;
                    if ((column - _numberOfBorderTiles) % expansionSize == 0)
                    {
                        if ((row - _numberOfBorderTiles) % expansionSize == 0)
                        {
                            newDirections[column, row] = direction;
                        }
                        else if ((direction & Direction.N) == Direction.N)
                        {
                            newDirections[column, row] = Direction.N | Direction.S;
                        }
                    }
                    else if ((row - _numberOfBorderTiles) % expansionSize == 0 && ((direction & Direction.E) == Direction.E))
                    {
                        newDirections[column, row] = Direction.E | Direction.W;
                    }
                }
            }

            return newDirections;
        }
    }
}