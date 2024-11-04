using CrawfisSoftware.Collections.Graph;

using System;
using System.Collections.Generic;

namespace CrawfisSoftware.Collections.Maze
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
    }
}