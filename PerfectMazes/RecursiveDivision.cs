using CrawfisSoftware.Maze;

using System;
using System.Collections.Generic;
using System.Text;

namespace CrawfisSoftware.Maze.PerfectMazes
{
    /// <summary>
    /// Extensions for IMazeBuilder for various perfect maze (spanning tree) algorithms.
    /// </summary>
    // todo: .Net Standard 3.0 and .Net 8.0 support partial static classes.
    // When Unity supports 3.0 we can make these partial and have the name w/o numbers.
    public static /*partial*/ class PerfectMaze3
    {
        /// <summary>
        /// Create a perfect maze using the Aldous Broder algorithm, which is a
        /// random walk and may take a while.
        /// </summary>
        /// <param name="mazeBuilder">A maze builder</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        /// <typeparam name="N">The type used for node labels</typeparam>
        /// <typeparam name="E">The type used for edge weights</typeparam>
        public static void RecursiveDivision<N, E>(this IMazeBuilder<N, E> mazeBuilder, bool preserveExistingCells = false)
        {
            var mazeBuilderRD = new MazeBuilderRecursiveDivision<N, E>(mazeBuilder);
            mazeBuilderRD.CarveMaze(preserveExistingCells);
        }
    }
}