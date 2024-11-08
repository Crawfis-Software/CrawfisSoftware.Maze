using CrawfisSoftware.Collections.Graph;

using System;

namespace CrawfisSoftware.Maze
{
    public class DirectionsInstrumented
    {
        protected Direction[,] _directions;

        public event Action<int, int, Direction, Direction> DirectionChanged;
        public Direction this[int column, int row]
        {
            get { return _directions[column, row]; }
            set
            {
                DirectionChanged?.Invoke(row, column, value, _directions[row, column]);
                _directions[column, row] = value;
            }
        }

        public Direction[,] Directions
        {
            get { return _directions; }
        }

        public DirectionsInstrumented(int width, int height)
        {
            _directions = new Direction[width, height];
        }

        public int GetLength(int dimension)
        {
            return _directions.GetLength(dimension);
        }
    }
}