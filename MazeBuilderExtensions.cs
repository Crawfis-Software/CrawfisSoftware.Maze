using CrawfisSoftware.Collections.Graph;
using CrawfisSoftware.Collections.Maze;

namespace CrawfisSoftware.Maze
{
    public static class MazeBuilderExtensions
    {
        public static bool CarveDirectionally<N, E>(this IMazeBuilder<N,E> mazeBuilder, int currentColumn, int currentRow, Direction directionToCarve, bool preserveExistingCells = false)
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
        public static bool CarvePassage<N, E>(this IMazeBuilder<N, E> mazeBuilder, int currentCell, int targetCell, bool preserveExistingCells = false)
        {
            int currentRow = currentCell / mazeBuilder.Width;
            int currentColumn = currentCell % mazeBuilder.Width;
            int selectedRow = targetCell / mazeBuilder.Width;
            int selectedColumn = targetCell % mazeBuilder.Width;
            return mazeBuilder.CarvePassage(currentColumn, currentRow, selectedColumn, selectedRow, preserveExistingCells);
        }

        /// <inheritdoc/>
        public static bool AddWall<N, E>(this IMazeBuilder<N, E> mazeBuilder, int currentCell, int targetCell, bool preserveExistingCells = false)
        {
            int currentRow = currentCell / mazeBuilder.Width;
            int currentColumn = currentCell % mazeBuilder.Width;
            int selectedRow = targetCell / mazeBuilder.Width;
            int selectedColumn = targetCell % mazeBuilder.Width;
            return mazeBuilder.AddWall(currentColumn, currentRow, selectedColumn, selectedRow, preserveExistingCells);
        }
    }
}