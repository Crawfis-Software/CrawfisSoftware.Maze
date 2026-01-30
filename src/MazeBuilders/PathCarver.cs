using CrawfisSoftware.Path;

namespace CrawfisSoftware.Maze
{
    /// <summary>
    /// Provides extension methods for carving paths into a maze using an IMazeBuilder instance.
    /// </summary>
    public static class PathCarver
    {
        /// <summary>
        /// Given a path, carve it into the maze.
        /// </summary>
        /// <param name="mazeBuilder">A maze builder</param>
        /// <param name="path">The path to carve into the maze</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        /// <typeparam name="N">The type used for node labels</typeparam>
        /// <typeparam name="E">The type used for edge weights</typeparam>
        public static void Carve<N, E>(this IMazeBuilder<N, E> mazeBuilder, GridPath<N, E> path, bool preserveExistingCells = false)
        {
            // Ensure the mazebuilder and the path.Grid have the same width and height
            if (mazeBuilder.Width != path.Grid.Width || mazeBuilder.Height != path.Grid.Height)
            {
                throw new ArgumentException("The maze builder and the path must have the same dimensions.");
            }

            // Carve the path into the maze
            int lastIndex = path[0];
            for (int i = 1; i < path.Count; i++)
            {
                int index = path[i];
                mazeBuilder.CarvePassage(lastIndex, index, preserveExistingCells);
                lastIndex = index;
            }
            if (path.IsClosed)
            {
                int index = path[0];
                mazeBuilder.CarvePassage(lastIndex, index, preserveExistingCells);
            }
        }
    }
}