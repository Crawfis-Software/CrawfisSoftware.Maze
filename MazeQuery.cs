using CrawfisSoftware.Collections.Graph;
using System.Collections.Generic;
using System.Linq;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Helper static class for listing cells with a certain configuration.
    /// </summary>
    public static class MazeQuery
    {
        /// <summary>
        /// Lists the grid (row,column) tuple of each dead-end.
        /// </summary>
        /// <param name="maze">The maze to query.</param>
        /// <returns>An IEnumberable os Tuples containing the row and column).</returns>
        public static IEnumerable<(int Row, int Column)> DeadEnds(this Maze<int, int> maze)
        {
            for (int row = 0; row < maze.Height; row++)
            {
                for (int column = 0; column < maze.Width; column++)
                {
                    Direction direction = maze.GetDirection(column, row);
                    if (direction.IsDeadEnd())
                    {
                        yield return (row, column);
                    }
                }
            }
        }

        /// <summary>
        /// Lists the grid (row,column) tuple of each TJunction.
        /// </summary>
        /// <param name="maze">The maze to query.</param>
        /// <returns>An IEnumberable os Tuples containing the row and column).</returns>
        public static IEnumerable<(int Row, int Column)> TJunctions(this Maze<int, int> maze)
        {
            //var query = from cell in maze
            //            where cell.NodeValue.IsTJunction()
            //            select (cell.Row, cell.Column);
            //return query;
            for (int row = 0; row < maze.Height; row++)
            {
                for (int column = 0; column < maze.Width; column++)
                {
                    Direction direction = maze.GetDirection(column, row);
                    if (direction.IsTJunction())
                    {
                        yield return (row, column);
                    }
                }
            }
        }

        /// <summary>
        /// Lists the grid (row,column) tuple of each horizontal or straight cell.
        /// </summary>
        /// <param name="maze">The maze to query.</param>
        /// <returns>An IEnumberable os Tuples containing the row and column).</returns>
        public static IEnumerable<(int Row, int Column)> Straights(this Maze<int, int> maze)
        {
            for (int row = 0; row < maze.Height; row++)
            {
                for (int column = 0; column < maze.Width; column++)
                {
                    Direction direction = maze.GetDirection(column, row);
                    if (direction.IsStraight())
                    {
                        yield return (row, column);
                    }
                }
            }
        }

        /// <summary>
        /// Lists the grid (row,column) tuple of each cell containing only a turn.
        /// </summary>
        /// <param name="maze">The maze to query.</param>
        /// <returns>An IEnumberable os Tuples containing the row and column).</returns>
        public static IEnumerable<(int Row, int Column)> Turns(this Maze<int, int> maze)
        {
            for (int row = 0; row < maze.Height; row++)
            {
                for (int column = 0; column < maze.Width; column++)
                {
                    Direction direction = maze.GetDirection(column, row);
                    if (direction.IsTurn())
                    {
                        yield return (row, column);
                    }
                }
            }
        }

        /// <summary>
        /// Lists the grid (row,column) tuple of each cross-section.
        /// </summary>
        /// <param name="maze">The maze to query.</param>
        /// <returns>An IEnumberable os Tuples containing the row and column).</returns>
        public static IEnumerable<(int Row, int Column)> CrossSections(this Maze<int, int> maze)
        {
            for (int row = 0; row < maze.Height; row++)
            {
                for (int column = 0; column < maze.Width; column++)
                {
                    Direction direction = maze.GetDirection(column, row);
                    if (direction.IsCrossSection())
                    {
                        yield return (row, column);
                    }
                }
            }
        }

        /// <summary>
        /// Lists the grid (row,column) tuple of each matching exactly the set of Directions.
        /// </summary>
        /// <param name="maze">The maze to query.</param>
        /// <param name="directions">An set of directions as a Direction Flag (enum).</param>
        /// <returns>An IEnumberable os Tuples containing the row and column).</returns>
        public static IEnumerable<(int Row, int Column)> MatchingExactly(this Maze<int, int> maze, Direction directions)
        {
            for (int row = 0; row < maze.Height; row++)
            {
                for (int column = 0; column < maze.Width; column++)
                {
                    Direction direction = maze.GetDirection(column, row);
                    if (direction == directions)
                    {
                        yield return (row, column);
                    }
                }
            }
        }

        /// <summary>
        /// Lists the grid (row,column) tuple of each cell that contain the set of directions.
        /// </summary>
        /// <param name="maze">The maze to query.</param>
        /// <param name="directions">An set of directions as a Direction Flag (enum).</param>
        /// <returns>An IEnumberable os Tuples containing the row and column).</returns>
        public static IEnumerable<(int Row, int Column)> ContainsAll(this Maze<int, int> maze, Direction directions)
        {
            for (int row = 0; row < maze.Height; row++)
            {
                for (int column = 0; column < maze.Width; column++)
                {
                    Direction direction = maze.GetDirection(column, row);
                    if ((direction & directions) == directions)
                    {
                        yield return (row, column);
                    }
                }
            }
        }

        /// <summary>
        /// Lists the grid (row,column) tuple of each cell that contain any of directions.
        /// </summary>
        /// <param name="maze">The maze to query.</param>
        /// <param name="directions">An set of directions as a Direction Flag (enum).</param>
        /// <returns>An IEnumberable os Tuples containing the row and column).</returns>
        public static IEnumerable<(int Row, int Column)> ContainsAny(this Maze<int, int> maze, Direction directions)
        {
            for (int row = 0; row < maze.Height; row++)
            {
                for (int column = 0; column < maze.Width; column++)
                {
                    Direction direction = maze.GetDirection(column, row);
                    if ((direction & directions) != Direction.None)
                    {
                        yield return (row, column);
                    }
                }
            }
        }

        // Todo: This needs testing. Useful after say removing deadends inconsistently to determine edges that need a barrier or something.
        // Todo: Handle the Undefined flag.
        public static IEnumerable<(int cell1, int cell2)> FindInconsistentEdges(this Maze<int, int> maze)
        {
            int width = maze.Width;
            int row, column;
            for (row = 1; row < maze.Height; row++)
            {
                for (column = 1; column < width; column++)
                {
                    if (!IsEastWestConsistentWithAffendingCell(maze, column, row, out (int,int) eastWestEdge))
                    {
                        yield return eastWestEdge;
                    }

                    if (!IsNorthSouthConsistentWithAffendingCell(maze, column, row, out (int, int) northSouthEdge))
                    {
                        yield return northSouthEdge;
                    }
                }
            }
            row = 0;
            for (column = 1; column < width; column++)
            {
                if (!IsEastWestConsistentWithAffendingCell(maze, column, row, out (int, int) eastWestEdge))
                {
                    yield return eastWestEdge;
                }
            }

            column = 0;
            for (row = 1; row < maze.Height; row++)
            {
                if (!IsNorthSouthConsistentWithAffendingCell(maze, column, row, out (int, int) northSouthEdge))
                {
                    yield return northSouthEdge;
                }
            }
        }

        static bool IsEastWestConsistentWithAffendingCell(Maze<int,int> maze, int column, int row, out (int,int) edge)
        {
            int width = maze.Width;
            int currentCell = row * width + column;
            int eastCell = currentCell - 1;
            Direction direction = maze.GetDirection(column, row);
            Direction eastDirs = maze.GetDirection(column-1, row);
            if ((direction & Direction.W) == Direction.W)
            {
                if ((eastDirs & Direction.E) != Direction.E)
                {
                    edge = (currentCell, eastCell);
                    return false;
                }
            }
            else if ((eastDirs & Direction.E) == Direction.E)
            {
                edge = (eastCell, currentCell);
                return false;
            }
            edge = (-1, -1);
            return true;
        }

        static bool IsNorthSouthConsistentWithAffendingCell(Maze<int, int> maze, int column, int row, out (int, int) edge)
        {
            int width = maze.Width;
            int currentCell = row * width + column;
            int southCell = currentCell - width;
            Direction direction = maze.GetDirection(column, row);
            Direction southDirs = maze.GetDirection(column, row-1);
            if ((direction & Direction.S) == Direction.S)
            {
                if ((southDirs & Direction.N) != Direction.N)
                {
                    edge = (currentCell, southCell);
                    return false;
                }
            }
            else if ((southDirs & Direction.N) == Direction.N)
            {
                edge = (southCell, currentCell);
                return false;
            }
            edge = (-1, -1);
            return true;
        }

        private static bool IsEastWestConsistent(Direction direction, Direction westNeighborDirections)
        {
            if ((direction & Direction.W) == Direction.W)
            {
                if ((westNeighborDirections & Direction.E) != Direction.E)
                    return false;
            }
            else if ((westNeighborDirections & Direction.E) == Direction.E)
            {
                return false;
            }
            return true;
        }

        private static bool IsNorthSouthConsistent(Direction direction, Direction southNeighborDirections)
        {
            if ((direction & Direction.S) == Direction.S)
            {
                if ((southNeighborDirections & Direction.N) != Direction.N)
                    return false;
            }
            else if ((southNeighborDirections & Direction.N) == Direction.N)
            {
                return false;
            }
            return true;
        }
    }
}