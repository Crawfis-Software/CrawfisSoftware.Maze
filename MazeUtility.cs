using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;

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
        /// <summary>
        /// Given a maze and a stamp set associating directions to stamps, create and return an occupancy grid.
        /// </summary>
        /// <typeparam name="N">The type used for node labels in the maze.</typeparam>
        /// <typeparam name="E">The type used for edge weights in the maze.</typeparam>
        /// <param name="maze">The maze.</param>
        /// <param name="stampSet">A stamp set with Direction as the associated type for id's.</param>
        /// <returns>An OccupancyGrid.</returns>
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

        /// <summary>
        /// Given a maze and a stamp set associating directions to stamps, create and return an occupancy grid.
        /// </summary>
        /// <typeparam name="N">The type used for node labels in the maze.</typeparam>
        /// <typeparam name="E">The type used for edge weights in the maze.</typeparam>
        /// <param name="maze">The maze.</param>
        /// <param name="stampSet">A stamp set with Direction as the associated type for id's.</param>
        /// <returns>An OccupancyGrid.</returns>
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

        /// <summary>
        /// Given a maze create and return an occupancy grid.
        /// </summary>
        /// <typeparam name="N">The type used for node labels in the maze.</typeparam>
        /// <typeparam name="E">The type used for edge weights in the maze.</typeparam>
        /// <param name="maze">The maze.</param>
        /// <param name="tileStyle">The style of the underlying stamp set to use. Default is to replace each cell with a 3x3 occupancy grid having all of the corners blocked.</param>
        /// <returns>An OccupancyGrid.</returns>
        public static OccupancyGrid CreateOccupancyGridFromMaze<N, E>(Maze<N, E> maze, TileStyle tileStyle = TileStyle.Tight3x3)
        {
            StampSet<Direction> stampSet;
            switch(tileStyle)
            {
                case TileStyle.Small2x2:
                    stampSet = CreateStampSet2x2Bool();
                    return CreateOccupancyGridFromMaze(maze, stampSet);
                    //break;
                case TileStyle.Open3x3:
                    stampSet = CreateStampSet3x3BoolOpen();
                    return ReplaceDirectionsWithStamps(maze, stampSet);
                    //return CreateOccupancyGridFromMaze(maze, stampSet);
                    //break;
                default:
                    stampSet = CreateStampSet3x3Bool();
                    return ReplaceDirectionsWithStamps(maze, stampSet);
                    //break;
            }
        }

        /// <summary>
        /// Create a 3x3 stamp set for all direction sets. All corners are closed.
        /// </summary>
        /// <returns>A stamp set of Direction's to OccupancyGrid's.</returns>
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

        /// <summary>
        /// Create a 3x3 stamp set for all direction sets. Corners shared by two openings are open.
        /// </summary>
        /// <returns>A stamp set of Direction's to OccupancyGrid's.</returns>
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

        /// <summary>
        /// Create a 2x2 stamp set for all direction sets. bottom-left corner is closed.
        /// </summary>
        /// <returns>A stamp set of Direction's to OccupancyGrid's.</returns>
        public static StampSet<Direction> CreateStampSet2x2Bool()
        {
            var stampSet = new StampSet<Direction>(2, 2, null);
            for (int i = 0; i <= 15; i++)
            {
                Direction direction = (Direction)i;
                var stamp = CreateStamp2x2(direction);
                stampSet.RegisterStamp(direction, stamp);
            }
            return stampSet;
        }

        /// <summary>
        /// Creates a StampSet foreach Direction where the right and top edges are open if there is a passage.
        /// </summary>
        /// <param name="width">The width of the stamps.</param>
        /// <param name="height">The heights of the stamps.</param>
        /// <param name="horizontalWallThickness">The thickness of the walls. Must be less than (width-1)/2 for any passages. Width minus twice this will be the passageway width.</param>
        /// <param name="verticalWallThickness">The thickness of the walls. Must be less than (height-1)/2 for any passages. Width minus twice this will be the passageway height.</param>
        /// <returns>A stamp set of Direction's to OccupancyGrid's</returns>
        public static StampSet<Direction> CreateStampSetOpenNxM(int width, int height, int horizontalWallThickness, int verticalWallThickness)
        {
            var stampSet = new StampSet<Direction>(width, height, null);
            for (int i = 0; i <= 15; i++)
            {
                Direction direction = (Direction)i;
                var stamp = CreateStampOpen(direction, width, height, horizontalWallThickness, verticalWallThickness);
                stampSet.RegisterStamp(direction, stamp);
            }
            return stampSet;

        }


        /// <summary>
        /// Creates a StampSet foreach Direction where the corners are closed off.
        /// </summary>
        /// <param name="width">The width of the stamps.</param>
        /// <param name="height">The heights of the stamps.</param>
        /// <param name="horizontalWallThickness">The thickness of the walls. Must be less than (width-1)/2 for any passages. Width minus twice this will be the passageway width.</param>
        /// <param name="verticalWallThickness">The thickness of the walls. Must be less than (height-1)/2 for any passages. Width minus twice this will be the passageway height.</param>
        /// <returns>A stamp set of Direction's to OccupancyGrid's</returns>
        public static StampSet<Direction> CreateStampSetClosedNxM(int width, int height, int horizontalWallThickness, int verticalWallThickness)
        {
            var stampSet = new StampSet<Direction>(width, height, null);
            for (int i = 0; i <= 15; i++)
            {
                Direction direction = (Direction)i;
                var stamp = CreateStampClosed(direction, width, height, horizontalWallThickness, verticalWallThickness);
                stampSet.RegisterStamp(direction, stamp);
            }
            return stampSet;

        }


        /// <summary>
        /// Creates a Stamp for a specified Direction where the right and top edges are open if there is a passage.
        /// </summary>
        /// <param name="direction">The Direction(s) that this stamp should support.</param>
        /// <param name="width">The width of the stamps.</param>
        /// <param name="height">The heights of the stamps.</param>
        /// <param name="horizontalWallThickness">The thickness of the walls. Must be less than (width-1)/2 for any passages. Width minus twice this will be the passageway width.</param>
        /// <param name="verticalWallThickness">The thickness of the walls. Must be less than (height-1)/2 for any passages. Width minus twice this will be the passageway height.</param>
        /// <returns>A stamp associated with the input Direction.</returns>
        public static OccupancyGrid CreateStampOpen(Direction direction, int width, int height, int horizontalWallThickness, int verticalWallThickness)
        {
            var occupancyGrid = new OccupancyGrid(width, height);
            if (direction == Direction.None) return occupancyGrid;
            occupancyGrid.Fill(true);
            if (!direction.HasFlag(Direction.S))
            {
                for (int row = 0; row < horizontalWallThickness; row++)
                {
                    for (int column = 0; column < width; column++)
                    {
                        occupancyGrid.MarkCell(column, row, false);
                    }
                }
            }

            if (!direction.HasFlag(Direction.W))
            {
                for (int row = 0; row < height; row++)
                {
                    for (int column = 0; column < verticalWallThickness; column++)
                    {
                        occupancyGrid.MarkCell(column, row, false);
                    }
                }
            }

            return occupancyGrid;
        }

        /// <summary>
        /// Creates a Stamp for a specified Direction where the corners are closed.
        /// </summary>
        /// <param name="direction">The Direction(s) that this stamp should support.</param>
        /// <param name="width">The width of the stamps.</param>
        /// <param name="height">The heights of the stamps.</param>
        /// <param name="horizontalWallThickness">The thickness of the walls. Must be less than (width-1)/2 for any passages. Width minus twice this will be the passageway width.</param>
        /// <param name="verticalWallThickness">The thickness of the walls. Must be less than (height-1)/2 for any passages. Width minus twice this will be the passageway height.</param>
        /// <returns>A stamp associated with the input Direction.</returns>
        public static OccupancyGrid CreateStampClosed(Direction direction, int width, int height, int horizontalWallThickness, int verticalWallThickness)
        {
            var occupancyGrid = new OccupancyGrid(width, height);
            if (direction == Direction.None) return occupancyGrid;
            occupancyGrid.Fill(true, verticalWallThickness, width-1-verticalWallThickness, horizontalWallThickness, height-1-horizontalWallThickness);
            if (direction.HasFlag(Direction.W))
            {
                occupancyGrid.Fill(true, 0, verticalWallThickness - 1, horizontalWallThickness, height - 1 - horizontalWallThickness);
            }
            if (direction.HasFlag(Direction.N))
            {
                occupancyGrid.Fill(true, verticalWallThickness, width - 1 - verticalWallThickness,  height - 1 - horizontalWallThickness, height-1);
            }
            if (direction.HasFlag(Direction.E))
            {
                occupancyGrid.Fill(true, width - 1 - verticalWallThickness, width-1, horizontalWallThickness, height - 1 - horizontalWallThickness);
            }
            if (direction.HasFlag(Direction.S))
            {
                occupancyGrid.Fill(true, verticalWallThickness, width - 1 - verticalWallThickness, 0, horizontalWallThickness - 1);
            }
            return occupancyGrid;
        }

        public static Maze<N, E> Inverse<N, E>(this Maze<N, E> maze)
        {
            var mazeBuilder = new MazeBuilderExplicit<N, E>(maze.Width, maze.Height);
            Direction allDirections = Direction.None;
            //foreach (Direction dir in Enum.GetValues<Direction>()) allDirections |= dir; // Generic version not available in .Net Standard 2.1
            foreach (var dir in Enum.GetValues(typeof(Direction))) allDirections |= (Direction) dir;
            for (int row = 0; row < maze.Height; row++)
            {
                for (int column = 0; column < maze.Width; column++)
                {
                    Direction directions = maze.GetDirection(column, row);
                    Direction inverseDirection = allDirections & ~directions;
                    mazeBuilder.SetCell(column, row, inverseDirection);
                }
            }
            mazeBuilder.RemoveUndefines();
            return mazeBuilder.GetMaze();
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

        private static OccupancyGrid CreateStamp2x2(Direction dir)
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