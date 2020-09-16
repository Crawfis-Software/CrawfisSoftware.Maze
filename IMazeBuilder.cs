using CrawfisSoftware.Collections.Graph;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Interface for creating mazes.
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    public interface IMazeBuilder<N, E>
    {
        /// <summary>
        /// The starting cell index for the maze. Cell indices go from bottom-left across a row to top-right.
        /// </summary>
        int StartCell { get; }

        /// <summary>
        /// The end cell index for the maze. Cell indices go from bottom-left across a row to top-right.
        /// </summary>
        int EndCell { get; }

        /// <summary>
        /// Get or set the random number generator that concrete maze builders may use.
        /// </summary>
        System.Random RandomGenerator { get; set; }

        /// <summary>
        /// Add an edge from <paramref name="currentCell"/> to <paramref name="targetCell"/> and vice versa.
        /// </summary>
        /// <param name="currentCell">A cell index</param>
        /// <param name="targetCell">A cell index</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        /// <returns>Returns true if the operation was successful.</returns>
        bool CarvePassage(int currentCell, int targetCell, bool preserveExistingCells = false);

        /// <summary>
        /// Remove any edge from <paramref name="currentCell"/> to <paramref name="targetCell"/> and vice versa.
        /// </summary>
        /// <param name="currentCell">A cell index</param>
        /// <param name="targetCell">A cell index</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        /// <returns>Returns true if the operation was successful.</returns>
        bool AddWall(int currentCell, int targetCell, bool preserveExistingCells = false);

        /// <summary>
        /// Delete all edges in the specified rectangle.
        /// </summary>
        /// <param name="lowerLeftCell">The cell index of the lower-left corner of a rectangular region</param>
        /// <param name="upperRightCell">The cell index of the upper-right corner of a rectangular region</param>
        void BlockRegion(int lowerLeftCell, int upperRightCell);

        /// <summary>
        /// Add all edges to neighbors within the specified rectangle.
        /// </summary>
        /// <param name="lowerLeftCell">The cell index of the lower-left corner of a rectangular region</param>
        /// <param name="upperRightCell">The cell index of the upper-right corner of a rectangular region</param>
        void OpenRegion(int lowerLeftCell, int upperRightCell);

        /// <summary>
        /// Build the maze based on the maze builder configuration
        /// </summary>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        void CreateMaze(bool preserveExistingCells = false);

        /// <summary>
        /// Get the current maze
        /// </summary>
        /// <returns>A maze</returns>
        Maze<N, E> GetMaze();

        /// <summary>
        /// Block the directions going to and from the cells
        /// </summary>
        /// <param name="currentColumn">A column index</param>
        /// <param name="currentRow">A row index</param>
        /// <param name="selectedColumn">Neighboring column index</param>
        /// <param name="selectedRow">Neighboring row index</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        /// <returns></returns>
        bool AddWall(int currentColumn, int currentRow, int selectedColumn, int selectedRow, bool preserveExistingCells = false);

        /// <summary>
        /// Carve a passage in the specified direction.
        /// </summary>
        /// <param name="currentColumn">Column index of the cell to carve</param>
        /// <param name="currentRow">Row index of the row to carve</param>
        /// <param name="directionToCarve">A single direction to carve</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        /// <return>Returns true if the operation was successful.</return>
        bool CarveDirectionally(int currentColumn, int currentRow, Direction directionToCarve, bool preserveExistingCells = false);

        /// <summary>
        /// Set all cells with the directions specified.
        /// </summary>
        /// <param name="lowerLeftCell">The lower-left corner of the region to fix.</param>
        /// <param name="upperRightCell">The upper-right corner of the region to fix.</param>
        /// <param name="dirs">List of directions to set each cell to.</param>
        /// <remarks>May lead to possible inconsistent neighbor directions.</remarks>
        /// <seealso>MakeBidirectionallyConsistent</seealso>
        void FillRegion(int lowerLeftCell, int upperRightCell, Direction dirs);

        /// <summary>
        /// Set all directions in the maze to Direction.Undefined
        /// </summary>
        void Clear();

        /// <summary>
        /// Remove dead-ends (implementation specific - one pass or many passes).
        /// Replaces dead-ends by blocking the only passage creating an empty cell
        /// </summary>
        /// <param name="preserveExistingCells"></param>
        void RemoveDeadEnds(bool preserveExistingCells = false);

        /// <summary>
        /// Remove Direction.Undefined for all cells that have been defined
        /// </summary>
        void FreezeCells();

        /// <summary>
        /// Remove Direction.Undefined for all cells.
        /// </summary>
        void RemoveUndefines();

        /// <summary>
        /// Ensures that all edges are bi-directional. In other words, a passage was not carved from A to
        /// B and not B to A.
        /// </summary>
        /// <remarks>This will open up all inconsistencies.</remarks>
        void MakeBidirectionallyConsistent();

        /// <summary>
        /// Ensures that all edges are bi-directional. In other words, a passage was not carved from A to
        /// B and not B to A.
        /// </summary>
        /// <param name="lowerLeftCell">The lower-left corner of the region to fix.</param>
        /// <param name="upperRightCell">The upper-right corner of the region to fix.</param>
        /// <remarks>This will open up all inconsistencies.</remarks>
        void MakeBiDirectionallyConsistent(int lowerLeftCell, int upperRightCell);

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
        /// Carve a continuous horizontal passage.
        /// </summary>
        /// <param name="row">The row to carve</param>
        /// <param name="column1">The start (or end) of the passage.</param>
        /// <param name="column2">The end (or start) of the passage.</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        void CarveHorizontalSpan(int row, int column1, int column2, bool preserveExistingCells);

        /// <summary>
        /// Carve a continuous vertical passage.
        /// </summary>
        /// <param name="column">The column to carve</param>
        /// <param name="row1">The start (or end) of the passage.</param>
        /// <param name="row2">The end (or start) of the passage.</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        void CarveVerticalSpan(int column, int row1, int row2, bool preserveExistingCells);
    }
}