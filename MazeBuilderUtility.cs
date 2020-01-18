using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace CrawfisSoftware.Collections.Maze
{
    public static class MazeBuilderUtility<N,E>
    {
        public static Maze<N, E> CreateMazeFromCSVFile(string filename, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
        {
            int width, height;
            List<Direction> directions;
            ReadCSVFile(filename, out width, out height, out directions);
            return CreateMaze(width, height, directions, nodeAccessor, edgeAccessor);
        }
        public static Maze<int,int> CreateMaze(int width, int height, List<Direction> directions)
        {
            return MazeBuilderUtility<int,int>.CreateMaze(width, height, directions, DummyNodeValues, DummyEdgeValues);
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

        public static void ReadCSVFile(string filename, out int width, out int height, out List<Direction> directions)
        {
            // Read in CSV file,
            // Send array to a MazeBuild
            // Get Maze and return it.
            directions = new List<Direction>();
            using (var file = new StreamReader(filename))
            {
                string row = file.ReadLine();
                string[] cells = row.Split(',');
                width = cells.Length;
                height = 1;
                foreach (var cell in cells)
                {
                    int cellValue = Int32.Parse(cell);
                    if (cellValue >= 0)
                    {
                        Direction dir = (Direction)Enum.Parse(typeof(Direction), cell);
                        directions.Add(dir);
                    }
                    else
                    {
                        // Note: Undefined could also be the value of 16.
                        directions.Add(Direction.Undefined);
                    }
                }
                row = file.ReadLine();
                while (row != null)
                {
                    height++;
                    cells = row.Split(',');
                    foreach (var cell in cells)
                    {
                        Direction dir;
                        bool invalidInput = false;
                        if (Enum.TryParse(cell, true, out dir))
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
                    row = file.ReadLine();
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
