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
                    if ((direction&directions) == directions)
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
    }
}