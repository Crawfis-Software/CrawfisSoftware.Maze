using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrawfisSoftware.Collections.Maze
{
    public static class MazeBuilderUtility<N, E>
    {
        public static Maze<N, E> CreateMazeFromCSVFile(string filename, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
        {
            int width, height;
            List<Direction> directions;
            ReadCSVFile(filename, out width, out height, out directions);
            Direction[,] directionGrid = MoveOriginToLowerLeft(width, height, directions);
            return CreateMaze(directionGrid, nodeAccessor, edgeAccessor);
        }

        public static Direction[,] MoveOriginToLowerLeft(int width, int height, List<Direction> directions)
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

        public static Maze<int, int> CreateMaze(int width, int height, List<Direction> directions)
        {
            return MazeBuilderUtility<int, int>.CreateMaze(width, height, directions, DummyNodeValues, DummyEdgeValues);
        }
        public static Maze<N, E> CreateMaze(int width, int height, List<Direction> directions, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
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

        public static void ReadCSVFile(string filename, out int width, out int height, out List<Direction> directions)
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

        public static int DummyNodeValues(int i, int j)
        {
            return 1;
        }
        public static int DummyEdgeValues(int i, int j, Direction dir)
        {
            return 1;
        }
    }
}
