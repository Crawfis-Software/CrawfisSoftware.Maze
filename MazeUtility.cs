using CrawfisSoftware.Collections.Graph;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Predefined OccupancyGrid stamp styles (tile styles).
    /// </summary>
    public enum TileStyle { 
        /// <summary>
        /// A 2x2 tile style for an OccupancyGrid.
        /// </summary>
        Small2x2,
        /// <summary>
        /// A 3x3 tile style with a cross for a cross-section.
        /// </summary>
        Tight3x3,
        /// <summary>
        /// A 3x3 tile style with edges more open for junctions.
        /// </summary>
        Open3x3 };

    /// <summary>
    /// Static methods to convert a Maze to an OccupancyGrid.
    /// </summary>
    public static class MazeUtility
    {
        public static OccupancyGrid ReplaceDirectionsWithStamps<N, E>(Maze<N, E> maze, StampSet<Direction> stampSet)
        {
            int width = stampSet.Width * maze.Width;
            int height = stampSet.Height * maze.Height;
            var newMaze = new OccupancyGrid(width, height);
            for (int row = 0; row < maze.Height; row++)
            {
                for (int column = 0; column < maze.Width; column++)
                {
                    var stamp = stampSet.GetStamp(maze.GetDirection(column, row));
                    GridUtility.StampInto(newMaze, stamp, stampSet.Width * column, stampSet.Height * row);
                }
            }
            return newMaze;
        }

        public static OccupancyGrid CreateOccupancyGridFromMaze<N, E>(Maze<N, E> maze, StampSet<Direction> stampSet)
        {
            var tempGrid = ReplaceDirectionsWithStamps<N, E>(maze, stampSet);
            var occupancyGraph = new OccupancyGrid(tempGrid.Width + 1, tempGrid.Height + 1);
            GridUtility.StampInto(occupancyGraph, tempGrid, 0, 0);
            for (int column = 0; column < maze.Width; column++)
            {
                if (maze.GetDirection(column, 0).HasFlag(Direction.S))
                {
                    occupancyGraph.MarkCell(stampSet.Width * column + 1, 0, true);
                    occupancyGraph.MarkCell(stampSet.Width * column + 1, 1, true);
                }
            }
            for (int row = 0; row < maze.Height; row++)
            {
                if (maze.GetDirection(maze.Width - 1, row).HasFlag(Direction.E))
                {
                    occupancyGraph.MarkCell(occupancyGraph.Width - 1, stampSet.Height * row + 1, true);
                    occupancyGraph.MarkCell(occupancyGraph.Width - 2, stampSet.Height * row + 1, true);
                }
            }
            return occupancyGraph;
        }

        public static OccupancyGrid CreateOccupancyGridFromMaze<N, E>(Maze<N, E> maze, TileStyle tileStyle)
        {
            StampSet<Direction> stampSet;
            switch(tileStyle)
            {
                case TileStyle.Small2x2:
                    stampSet = CreateStampSet2x2Bool();
                    break;
                case TileStyle.Open3x3:
                    stampSet = CreateStampSet3x3BoolOpen();
                    break;
                default:
                    stampSet = CreateStampSet3x3Bool();
                    break;
            }
            return CreateOccupancyGridFromMaze(maze, stampSet);
        }

        public static StampSet<Direction> CreateStampSet3x3Bool()
        {
            var stampSet = new StampSet<Direction>(3, 3, null);
            for (int i = 0; i <= 15; i++)
            {
                Direction direction = (Direction)i;
                var stamp = CreateStampClosed3x3(direction);
                stampSet.RegisterStamp(direction, stamp);
            }
            return stampSet;
        }

        public static StampSet<Direction> CreateStampSet3x3BoolOpen()
        {
            var stampSet = new StampSet<Direction>(3, 3, null);
            for (int i = 0; i <= 15; i++)
            {
                Direction direction = (Direction)i;
                var stamp = CreateStampOpen3x3(direction);
                stampSet.RegisterStamp(direction, stamp);
            }
            return stampSet;
        }

        public static StampSet<Direction> CreateStampSet2x2Bool()
        {
            var stampSet = new StampSet<Direction>(2, 2, null);
            for (int i = 0; i <= 15; i++)
            {
                Direction direction = (Direction)i;
                var stamp = CreateStampClosed2x2(direction);
                stampSet.RegisterStamp(direction, stamp);
            }
            return stampSet;
        }

        private static OccupancyGrid CreateStampOpen3x3(Direction dir)
        {
            var occupancyGrid = new OccupancyGrid(3, 3);
            occupancyGrid.Fill(true);
            bool closedCell = (dir == Direction.None);
            bool west = (dir & Direction.W) == Direction.W;
            bool north = (dir & Direction.N) == Direction.N;
            bool east = (dir & Direction.E) == Direction.E;
            bool south = (dir & Direction.S) == Direction.S;
            // If no entrances, seal the center
            occupancyGrid.MarkCell(1, 1, !closedCell);
            // If no entrance from the west, add a left wall.
            if (!west)
            {
                occupancyGrid.MarkCell(0, 0, false);
                occupancyGrid.MarkCell(0, 1, false);
                occupancyGrid.MarkCell(0, 2, false);
            }
            // If no entrance from the south, add a bottom wall
            if (!south)
            {
                occupancyGrid.MarkCell(0, 0, false);
                occupancyGrid.MarkCell(1, 0, false);
                occupancyGrid.MarkCell(2, 0, false);
            }
            // If no entrance from the east, add a right wall
            if (!east)
            {
                occupancyGrid.MarkCell(2, 0, false);
                occupancyGrid.MarkCell(2, 1, false);
                occupancyGrid.MarkCell(2, 2, false);
            }
            // If no entrance from the south, add a top wall
            if (!north)
            {
                occupancyGrid.MarkCell(0, 2, false);
                occupancyGrid.MarkCell(1, 2, false);
                occupancyGrid.MarkCell(2, 2, false);
            }
            return occupancyGrid;
        }

        private static OccupancyGrid CreateStampClosed3x3(Direction dir)
        {
            var occupancyGrid = new OccupancyGrid(3, 3);
            bool any = (dir != Direction.None);
            bool west = (dir & Direction.W) == Direction.W;
            bool north = (dir & Direction.N) == Direction.N;
            bool east = (dir & Direction.E) == Direction.E;
            bool south = (dir & Direction.S) == Direction.S;
            // The following lines that set the grid to false are not needed, as it is false by default with C#.
            occupancyGrid.MarkCell(0, 0, false);
            occupancyGrid.MarkCell(1, 0, south);
            occupancyGrid.MarkCell(2, 0, false);
            occupancyGrid.MarkCell(0, 1, west);
            occupancyGrid.MarkCell(1, 1, any);
            occupancyGrid.MarkCell(2, 1, east);
            occupancyGrid.MarkCell(0, 2, false);
            occupancyGrid.MarkCell(1, 2, north);
            occupancyGrid.MarkCell(2, 2, false);
            return occupancyGrid;
        }

        private static OccupancyGrid CreateStampClosed2x2(Direction dir)
        {
            var occupancyGrid = new OccupancyGrid(2, 2);
            bool any = (dir != Direction.None);
            bool west = (dir & Direction.W) == Direction.W;
            bool south = (dir & Direction.S) == Direction.S;
            occupancyGrid.MarkCell(0, 1, west);
            occupancyGrid.MarkCell(1, 1, any);
            occupancyGrid.MarkCell(1, 0, south);
            return occupancyGrid;
        }
    }
}