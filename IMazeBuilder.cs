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
    }
}