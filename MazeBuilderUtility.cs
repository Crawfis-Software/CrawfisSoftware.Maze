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
            // Read in CSV file,
            // Send array to a MazeBuild
            // Get Maze and return it.
            int width, height;
            List<Direction> directions = new List<Direction>();
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
                        int cellValue = Int32.Parse(cell);
                        if (cellValue >= 0)
                        {
                            // Parse will only throw an exception if using strings (e.g. "N,NW"), not numbers (e.g, 16).
                            Direction dir = (Direction)Enum.Parse(typeof(Direction), cell);
                            if (!Enum.IsDefined(typeof(Direction), dir))
                            {
                                string error = string.Format("The value {0} being read in the CSV file {1} is not a valid set of Directions", dir, filename);
                                throw new InvalidCastException(error);
                            }
                            directions.Add(dir);
                        }
                        else
                        {
                            directions.Add(Direction.Undefined);
                        }
                    }
                    row = file.ReadLine();
                }
            }
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
