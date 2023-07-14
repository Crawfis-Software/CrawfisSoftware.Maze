using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Static class for some useful tools to build mazes
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    public static class MazeBuilderUtility<N, E>
    {
        /// <summary>
        /// Create a maze from a CSV file
        /// </summary>
        /// <param name="filename">Path and filename of the csv file to open.</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        /// <returns></returns>
        public static Maze<N, E> CreateMazeFromCSVFile(string filename, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
        {
            int width, height;
            List<Direction> directions;
            ReadCSVFile(filename, out width, out height, out directions);
            Direction[,] directionGrid = MoveOriginToLowerLeft(width, height, directions);
            return CreateMaze(directionGrid, nodeAccessor, edgeAccessor);
        }

        /// <summary>
        /// Utility on Direction that will take a stream of directions with the y origin as the top
        /// and produce a 2D grid of Directions with the y origin as the bottom row.
        /// </summary>
        /// <param name="width">The desired width of the maze</param>
        /// <param name="height">The desired height of the maze</param>
        /// <param name="directions">A stream of directions starting from the lower-left corner.</param>
        /// <returns>A 2D grid of Directions</returns>
        public static Direction[,] MoveOriginToLowerLeft(int width, int height, IList<Direction> directions)
        {
            Direction[,] directionGrid = new Direction[width, height];
            int row = height - 1;
            int column = 0;
            foreach (Direction dir in directions)
            {
                directionGrid[column, row] = dir;
                column++;
                if (column >= width)
                {
                    column = 0;
                    row--;
                }
            }
            return directionGrid;
        }

        /// <summary>
        /// Create a maze given a stream of directions
        /// </summary>
        /// <param name="width">The desired width of the maze</param>
        /// <param name="height">The desired height of the maze</param>
        /// <param name="directions">A stream of directions starting from the lower-left corner.</param>
        /// <returns>A new maze</returns>
        public static Maze<int, int> CreateMaze(int width, int height, IList<Direction> directions)
        {
            return MazeBuilderUtility<int, int>.CreateMaze(width, height, directions, 
                MazeBuilderUtility<int,int>.DummyNodeValues, MazeBuilderUtility<int, int>.DummyEdgeValues);
        }

        /// <summary>
        /// Create a maze given a stream of directions
        /// </summary>
        /// <param name="width">The desired width of the maze</param>
        /// <param name="height">The desired height of the maze</param>
        /// <param name="directions">A stream of directions starting from the lower-left corner.</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        /// <returns>A new maze</returns>
        public static Maze<N, E> CreateMaze(int width, int height, IList<Direction> directions, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
        {
            var mazeBuilder = new MazeBuilderExplicit<N, E>(width, height, nodeAccessor, edgeAccessor);
            int i = 0;
            int j = 0;
            foreach (var dir in directions)
            {
                mazeBuilder.SetCell(i, j, dir);
                i++;
                if (i >= width)
                {
                    i = 0;
                    j++;
                }
            }
            return mazeBuilder.GetMaze();
        }

        /// <summary>
        /// Create a maze from a grid of Directions
        /// </summary>
        /// <param name="directions">A 2D array of Directions for the maze</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        /// <returns>A maze</returns>
        public static Maze<N, E> CreateMaze(Direction[,] directions, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
        {
            int width = directions.GetLength(0);
            int height = directions.GetLength(1);
            var mazeBuilder = new MazeBuilderExplicit<N, E>(width, height, nodeAccessor, edgeAccessor);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    mazeBuilder.SetCell(i, j, directions[i, j]);
                }
            }
            return mazeBuilder.GetMaze();
        }

        private static void ReadCSVFile(string filename, out int width, out int height, out List<Direction> directions)
        {
            // Read in CSV file,
            // Send array to a MazeBuild
            // Get Maze and return it.
            directions = new List<Direction>();
            using (var file = new StreamReader(filename))
            {
                string row = file.ReadLine();
                height = 1;
                row = file.ReadLine();
                ReadDirections(directions, row);
                width = directions.Count;
                row = file.ReadLine();
                while (row != null)
                {
                    height++;
                    ReadDirections(directions, row);
                    row = file.ReadLine();
                }
            }
        }

        private static void ReadDirections(List<Direction> directions, string row)
        {
            string[] cells = row.Split(',');
            foreach (var cell in cells)
            {
                Direction dir;
                bool invalidInput = false;
                // Convert cell's or format to a comma separated list.
                string cellDirs = cell.Replace("|", ",");
                if (Enum.TryParse(cellDirs, true, out dir))
                {
                    // Parse will only return false if there is an invalid string (e.g. "N,NW"), not an invalid number (e.g, 234).
                    if (Enum.IsDefined(typeof(Direction), dir) | dir.ToString().Contains(","))
                    {
                        directions.Add(dir);
                    }
                    else
                    {
                        invalidInput = true;
                    }
                }
                else
                {
                    invalidInput = true;
                }
                if (invalidInput)
                {
                    //string error = string.Format("The value {0} being read in the CSV file {1} is not a valid set of Directions", dir, filename);
                    //throw new InvalidCastException(error);
                    directions.Add(Direction.Undefined);
                }
            }
        }

        /// <summary>
        /// Function that always returns 1
        /// </summary>
        /// <param name="i">Column index of a cell.</param>
        /// <param name="j">Row index of a cell.</param>
        /// <returns>The default value for the <typeparamref name="N"/>.</returns>
        public static N DummyNodeValues(int i, int j)
        {
            return default(N);
        }

        /// <summary>
        /// Function that always returns the default value
        /// </summary>
        /// <param name="i">Column index of a cell.</param>
        /// <param name="j">Row index of a cell.</param>
        /// <param name="dir">Direction of the desired edge</param>
        /// <returns>The default value for the <typeparamref name="E"/>.</returns>
        public static E DummyEdgeValues(int i, int j, Direction dir)
        {
            return default(E);
        }
    }
}
