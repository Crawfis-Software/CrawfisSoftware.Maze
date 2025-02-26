using CrawfisSoftware.Collections.Graph;

namespace CrawfisSoftware.Maze
{
    /// <summary>
    /// Interface for creating mazes.
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    public interface IMazeBuilder<N, E>
    {
        /// <summary>
        /// Get the width in the number of grid cells
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Get the height in the number of grid cells
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// The starting cell index for the maze. Cell indices go from bottom-left across a row to top-right.
        /// </summary>
        int StartCell { get; set; }

        /// <summary>
        /// The end cell index for the maze. Cell indices go from bottom-left across a row to top-right.
        /// </summary>
        int EndCell { get; set; }

        /// <summary>
        /// Get or set the random number generator that concrete maze builders may use.
        /// </summary>
        System.Random RandomGenerator { get; set; }

        /// <summary>
        /// Get the underlying grid data structure
        /// </summary>
        Grid<N, E> Grid { get; }

        /// <summary>
        /// Get the current maze
        /// </summary>
        /// <returns>A maze</returns>
        Maze<N, E> GetMaze();

        /// <summary>
        /// Get the direction for the specified cell.
        /// </summary>
        /// <param name="column">The column index of the cell.</param>
        /// <param name="row">The row index of the cell.</param>
        /// <returns>The Direction flags.</returns>
        Direction GetDirection(int column, int row);

        /// <summary>
        /// Set the directions for this cell w/o any safeguards
        /// </summary>
        /// <param name="i">The column index</param>
        /// <param name="j">The row index</param>
        /// <param name="dirs">The cell value including all directions</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        void SetCell(int i, int j, Direction dirs, bool preserveExistingCells = false);

        /// <summary>
        /// Carve a passage in the specified direction.
        /// </summary>
        /// <param name="currentColumn">A column index</param>
        /// <param name="currentRow">A row index</param>
        /// <param name="selectedColumn">Neighboring column index</param>
        /// <param name="selectedRow">Neighboring row index</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        /// <return>Returns true if the operation was successful.</return>
        bool CarvePassage(int currentColumn, int currentRow, int selectedColumn, int selectedRow, bool preserveExistingCells = false);

        /// <summary>
        /// Block the directions going to and from the cells
        /// </summary>
        /// <param name="currentColumn">A column index</param>
        /// <param name="currentRow">A row index</param>
        /// <param name="selectedColumn">Neighboring column index</param>
        /// <param name="selectedRow">Neighboring row index</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        /// <returns>True if the wall was able to be added.</returns>
        bool AddWall(int currentColumn, int currentRow, int selectedColumn, int selectedRow, bool preserveExistingCells = false);

        /// <summary>
        /// Set all directions in the maze to Direction.Undefined
        /// </summary>
        void Clear();

        /// <summary>
        /// Remove Direction.Undefined for all cells.
        /// </summary>
        void RemoveUndefines();

        /// <summary>
        /// Remove the Undefined flag from the specified cell.
        /// </summary>
        /// <param name="row">A row index</param>
        /// <param name="column">A column index</param>
        void RemoveUndefine(int row, int column);

        /// <summary>
        /// Loops over the specified region and removes the Direction.Undefined if any.
        /// </summary>
        /// <param name="currentColumn">Lower-left column</param>
        /// <param name="currentRow">Lower-left row</param>
        /// <param name="endColumn">upper-left column inclusive</param>
        /// <param name="endRow">upper-right row inclusive</param>
        /// <param name="carvingMissingPassages">If true, fix inconsistencies by opening up both sides. If false, wall up  both sides.</param>
        void MakeBidirectionallyConsistent(int currentColumn, int currentRow, int endColumn, int endRow, bool carvingMissingPassages = true);

        /// <summary>
        /// Remove the Undefined (freeze) the specified cell if it has a Direction set.
        /// </summary>
        /// <param name="row">A row index</param>
        /// <param name="column">A column index</param>
        void FreezeCellIfUndefined(int row, int column);
        void RemoveDirections(int i, int j, Direction dirs, bool preserveExistingCells = false);
        void RemoveDirectionsExplicitly(int i, int j, Direction dirs);

        void AddDirections(int i, int j, Direction dirs, bool preserveExistingCells = false);
        /// <summary>
        /// Add the direction(s) to this cell w/o any safeguards
        /// </summary>
        /// <param name="i">The column index</param>
        /// <param name="j">The row index</param>
        /// <param name="dirs">The directions to add</param>
        void AddDirectionExplicitly(int i, int j, Direction dirs);
    }
}