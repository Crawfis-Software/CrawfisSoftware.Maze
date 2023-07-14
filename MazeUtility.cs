using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CrawfisSoftware.Collections.Maze
{
    public enum TileStyle { Small2x2, Tight3x3, Open3x3, Wrong3x3 };

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

        //public static IGridGraph<bool> Create3x3OccupancyGridFromMaze(IGridGraph<Direction> maze, TileStyle openStyle = TileStyle.Tight3x3)
        //{
        //    var stampSet = CreateStampSet3x3Bool(openStyle);
        //    return ReplaceDirectionsWithStamps(maze, stampSet);
        //}

        public static OccupancyGrid Create2x2OccupancyGridFromMaze<N, E>(Maze<N,E> maze)
        {
            var stampSet = CreateStampSet2x2Bool();
            var tempGrid = ReplaceDirectionsWithStamps<N, E>(maze, stampSet);
            var occupancyGraph = new OccupancyGrid(tempGrid.Width + 1, tempGrid.Height + 1);
            GridUtility.StampInto(occupancyGraph, tempGrid, 0, 1);
            for (int column = 0; column < maze.Width; column++)
            {
                if (maze.GetDirection(column, 0).HasFlag(Direction.S))
                {
                    occupancyGraph.MarkCell(2 * column + 1, 0, true);
                    occupancyGraph.MarkCell(2 * column + 1, 1, true);
                }
            }
            for (int row = 0; row < maze.Height; row++)
            {
                if (maze.GetDirection(maze.Width - 1, row).HasFlag(Direction.E))
                {
                    occupancyGraph.MarkCell(occupancyGraph.Width - 1, 2 * row, true);
                    occupancyGraph.MarkCell(occupancyGraph.Width - 2, 2 * row, true);
                }
            }
            return occupancyGraph;
        }

        //public static void FillAllCells<E>(this GridGraph<E> occupancyGrid, E fillValue)
        //{
        //    for (int row = 0; row < occupancyGrid.NumberOfRows; row++)
        //    {
        //        for (int column = 0; column < occupancyGrid.NumberOfColumns; column++)
        //        {
        //            occupancyGrid.SetCellValue(row, column, fillValue);
        //        }
        //    }
        //}

        //public static StampSet<Direction, bool> CreateStampSet3x3Bool(TileStyle openStyle)
        //{
        //    Func<Direction, GridGraph<bool>> createStampFunc = CreateStampClosed3x3;
        //    if (openStyle == TileStyle.Open3x3)
        //        createStampFunc = CreateStampOpen3x3;
        //    if (openStyle == TileStyle.Wrong3x3)
        //        createStampFunc = CreateStampOpenMore3x3;

        //    var stampSet = new StampSet<Direction, bool>(3, 3);
        //    for (int i = 0; i <= 15; i++)
        //    {
        //        Direction direction = (Direction)i;
        //        var stamp = createStampFunc(direction);
        //        stampSet.RegisterStamp(direction, stamp);
        //    }
        //    return stampSet;
        //}

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

        //private static GridGraph<bool> CreateStampOpen3x3(Direction dir)
        //{
        //    var backingStore = new GridNodeDataStore<bool>(3, 3);
        //    var occupancyGrid = new GridGraph<bool>(3, 3, backingStore);
        //    occupancyGrid.FillAllCells(true);
        //    bool closedCell = (dir == Direction.Blank);
        //    bool west = (dir & Direction.West) == Direction.West;
        //    bool north = (dir & Direction.North) == Direction.North;
        //    bool east = (dir & Direction.East) == Direction.East;
        //    bool south = (dir & Direction.South) == Direction.South;
        //    // If no entrances, seal the center
        //    occupancyGrid.SetCellValue(1, 1, !closedCell);
        //    // If no entrance from the west, add a left wall.
        //    if (!west)
        //    {
        //        occupancyGrid.SetCellValue(0, 0, false);
        //        occupancyGrid.SetCellValue(1, 0, false);
        //        occupancyGrid.SetCellValue(2, 0, false);
        //    }
        //    // If no entrance from the south, add a bottom wall
        //    if (!south)
        //    {
        //        occupancyGrid.SetCellValue(0, 0, false);
        //        occupancyGrid.SetCellValue(0, 1, false);
        //        occupancyGrid.SetCellValue(0, 2, false);
        //    }
        //    // If no entrance from the east, add a right wall
        //    if (!east)
        //    {
        //        occupancyGrid.SetCellValue(0, 2, false);
        //        occupancyGrid.SetCellValue(1, 2, false);
        //        occupancyGrid.SetCellValue(2, 2, false);
        //    }
        //    // If no entrance from the north, add a top wall
        //    if (!north)
        //    {
        //        occupancyGrid.SetCellValue(2, 0, false);
        //        occupancyGrid.SetCellValue(2, 1, false);
        //        occupancyGrid.SetCellValue(2, 2, false);
        //    }
        //    return occupancyGrid;
        //}

        //// This will produce non-valid tiles for a perfect maze, but interesting dungeons.
        //private static GridGraph<bool> CreateStampOpenMore3x3(Direction dir)
        //{
        //    var backingStore = new GridNodeDataStore<bool>(3, 3);
        //    var occupancyGrid = new GridGraph<bool>(3, 3, backingStore);
        //    //occupancyGrid.FillAllCells(true);
        //    bool closedCell = (dir == Direction.Blank);
        //    bool west = (dir & Direction.West) == Direction.West;
        //    bool north = (dir & Direction.North) == Direction.North;
        //    bool east = (dir & Direction.East) == Direction.East;
        //    bool south = (dir & Direction.South) == Direction.South;
        //    // If no entrances, seal the center
        //    occupancyGrid.SetCellValue(1, 1, !closedCell);
        //    // If no entrance from the west, add a left wall.
        //    if (west)
        //    {
        //        occupancyGrid.SetCellValue(0, 0, true);
        //        occupancyGrid.SetCellValue(1, 0, true);
        //        occupancyGrid.SetCellValue(2, 0, true);
        //    }
        //    // If no entrance from the south, add a bottom wall
        //    if (south)
        //    {
        //        occupancyGrid.SetCellValue(0, 0, true);
        //        occupancyGrid.SetCellValue(0, 1, true);
        //        occupancyGrid.SetCellValue(0, 2, true);
        //    }
        //    // If no entrance from the east, add a right wall
        //    if (east)
        //    {
        //        occupancyGrid.SetCellValue(0, 2, true);
        //        occupancyGrid.SetCellValue(1, 2, true);
        //        occupancyGrid.SetCellValue(2, 2, true);
        //    }
        //    // If no entrance from the north, add a top wall
        //    if (north)
        //    {
        //        occupancyGrid.SetCellValue(2, 0, true);
        //        occupancyGrid.SetCellValue(2, 1, true);
        //        occupancyGrid.SetCellValue(2, 2, true);
        //    }
        //    return occupancyGrid;
        //}

        //private static GridGraph<bool> CreateStampClosed3x3(Direction dir)
        //{
        //    var backingStore = new GridNodeDataStore<bool>(3, 3);
        //    var occupancyGrid = new GridGraph<bool>(3, 3, backingStore);
        //    bool any = (dir != Direction.Blank);
        //    bool west = (dir & Direction.West) == Direction.West;
        //    bool north = (dir & Direction.North) == Direction.North;
        //    bool east = (dir & Direction.East) == Direction.East;
        //    bool south = (dir & Direction.South) == Direction.South;
        //    // The following lines that set the grid to false are not needed, as it is false by default with C#.
        //    occupancyGrid.SetCellValue(0, 0, false);
        //    occupancyGrid.SetCellValue(0, 1, south);
        //    occupancyGrid.SetCellValue(0, 2, false);
        //    occupancyGrid.SetCellValue(1, 0, west);
        //    occupancyGrid.SetCellValue(1, 1, any);
        //    occupancyGrid.SetCellValue(1, 2, east);
        //    occupancyGrid.SetCellValue(2, 0, false);
        //    occupancyGrid.SetCellValue(2, 1, north);
        //    occupancyGrid.SetCellValue(2, 2, false);
        //    return occupancyGrid;
        //}

        private static OccupancyGrid CreateStampClosed2x2(Direction dir)
        {
            var occupancyGrid = new OccupancyGrid(2, 2);
            bool any = (dir != Direction.None);
            bool west = (dir & Direction.W) == Direction.W;
            bool north = (dir & Direction.N) == Direction.N;
            occupancyGrid.MarkCell(0, 0, west);
            occupancyGrid.MarkCell(1, 0, any);
            occupancyGrid.MarkCell(1, 1, north);
            return occupancyGrid;
        }
    }
}