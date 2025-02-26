using CrawfisSoftware.Collections.Graph;

using System;
using System.Collections.Generic;

namespace CrawfisSoftware.Maze
{
    /// <summary>
    /// Predefined OccupancyGrid stamp styles (tile styles).
    /// </summary>
    public enum TileStyle
    {
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
        Open3x3
    };

    /// <summary>
    /// Static methods to convert a Maze to an OccupancyGrid.
    /// </summary>
    public static class MazeOccupancyGrids
    {
        /// <summary>
        /// Given a maze and a stamp set associating directions to stamps, create and return an occupancy grid.
        /// </summary>
        /// <typeparam name="N">The type used for node labels in the maze.</typeparam>
        /// <typeparam name="E">The type used for edge weights in the maze.</typeparam>
        /// <param name="maze">The maze.</param>
        /// <param name="stampSet">A stamp set with Direction as the associated type for id's.</param>
        /// <param name="ignoreUndefinedDirection">If true, undefined directions will be ignored.</param>
        /// <returns>An OccupancyGrid.</returns>
        public static OccupancyGrid ReplaceDirectionsWithStamps<N, E>(Maze<N, E> maze, StampSet<Direction> stampSet, bool ignoreUndefinedDirection = true)
        {
            int width = stampSet.Width * maze.Width;
            int height = stampSet.Height * maze.Height;
            var newMaze = new OccupancyGrid(width, height);
            for (int row = 0; row < maze.Height; row++)
            {
                for (int column = 0; column < maze.Width; column++)
                {
                    Direction direction = maze.GetDirection(column, row);
                    if (ignoreUndefinedDirection) direction = direction & ~Direction.Undefined;
                    var stamp = stampSet.GetStamp(direction);
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
            switch (tileStyle)
            {
                case TileStyle.Small2x2:
                    stampSet = StampSetFactory.CreateStampSet2x2Bool();
                    return CreateOccupancyGridFromMaze(maze, stampSet);
                //break;
                case TileStyle.Open3x3:
                    stampSet = StampSetFactory.CreateStampSet3x3BoolOpen();
                    return ReplaceDirectionsWithStamps(maze, stampSet);
                //return CreateOccupancyGridFromMaze(maze, stampSet);
                //break;
                default:
                    stampSet = StampSetFactory.CreateStampSet3x3Bool();
                    return ReplaceDirectionsWithStamps(maze, stampSet);
                    //break;
            }
        }
        /// <summary>
        /// Carve openings based on the list of compressed vertical and horizontal edge flags for each row
        /// </summary>
        /// <param name="mazeBuilder">An existing maze builder to use in the carving process</param>
        /// <param name="solidBlocks">2D array matching the maze builder's width and height.
        /// A value of true implies this cell is a solid block. Passages will be carved from non-solid
        /// blocks to adjacent non-solid blocks.</param>
        public static void CarveOpenings(IMazeBuilder<int, int> mazeBuilder, bool[,] solidBlocks)
        {

            for (int row = 0; row < mazeBuilder.Height - 1; row++)
            {
                for (int column = 0; column < mazeBuilder.Width - 1; column++)
                {
                    if (!solidBlocks[column, row])
                    {
                        if (!solidBlocks[column, row + 1])
                        {
                            mazeBuilder.CarvePassage(column, row, column, row + 1);
                        }
                        if (!solidBlocks[column + 1, row])
                        {
                            mazeBuilder.CarvePassage(column, row, column + 1, row);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Convert an OccupancyGrid to a Maze.
        /// </summary>
        /// <param name="cells">An <c>OccupancyGrid</c>.</param>
        /// <returns>A <c>Maze</c>.</returns>
        public static Maze<int, int> ConvertOccupancyGridToMaze(this OccupancyGrid cells)
        {
            var mazeBuilder = new MazeBuilder<int, int>(cells.Width, cells.Height);
            CarveOpenings(mazeBuilder, cells.GridValues);
            Maze<int, int> maze = mazeBuilder.GetMaze();
            return maze;
        }
    }
}