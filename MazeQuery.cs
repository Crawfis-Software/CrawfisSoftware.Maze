using CrawfisSoftware.Collections.Graph;
using System.Collections.Generic;
using System.Data.Common;
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
        /// <returns>An IEnumerable of Tuples containing the row and column.</returns>
        public static IEnumerable<(int Row, int Column)> DeadEnds<N, E>(this Maze<N, E> maze)
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
        /// <returns>An IEnumerable of Tuples containing the row and column.</returns>
        public static IEnumerable<(int Row, int Column)> TJunctions<N, E>(this Maze<N, E> maze)
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
        /// Lists the grid (row,column) tuple of each horizontal or vertical cell.
        /// </summary>
        /// <param name="maze">The maze to query.</param>
        /// <returns>An IEnumerable of Tuples containing the row and column.</returns>
        public static IEnumerable<(int Row, int Column)> Straights<N, E>(this Maze<N, E> maze)
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
        /// Lists the grid (row,column) tuple of each start of a sequence of horizontal or vertical cells.
        /// </summary>
        /// <param name="maze">The maze to query.</param>
        /// <returns>An IEnumerable of Tuples containing the starting cell index, the direction and the length of the straightaway.</returns>
        /// <remarks>Uses Depth-First Search from the Maze's starting cell.</remarks>
        public static IEnumerable<(int CellIndex, Direction StraightAwayDirection, int StraightAwayLength)> StraightAways<N, E>(this Maze<N, E> maze)
        {
            int start = maze.StartCell;
            var dfs = new IndexedGraphEdgeEnumerator<N, E>(maze);
            int currentStraightAwayIndex = -1;
            int currentStraightAwayLength = 0;
            Direction currentDirection = Direction.None;
            int lastCellIndex = -1;
            int width = maze.Width;
            int row = start / width;
            int column = start % width;
            Direction direction = maze.GetDirection(column, row);
            if (direction.IsStraight())
            {
                currentStraightAwayIndex = start;
                currentStraightAwayLength = 1;
                currentDirection = Direction.N;
                lastCellIndex = start;
            }
            foreach (var edge in dfs.TraverseGraph(start))
            {
                row = edge.To / width;
                column = edge.To % width;
                direction = maze.GetDirection(column, row);
                if (direction.IsStraight())
                {
                    // This is either a continuation or a new straight away.
                    if(edge.From == lastCellIndex)
                    {
                        currentStraightAwayLength++;
                        lastCellIndex = edge.To;
                    }
                    else
                    {
                        if(currentStraightAwayIndex >= 0)
                            yield return (currentStraightAwayIndex, currentDirection, currentStraightAwayLength);
                        currentStraightAwayIndex = edge.To;
                        currentStraightAwayLength = 1;
                        currentDirection = DirectionExtensions.GetEdgeDirection(edge.From, edge.To, width);
                        lastCellIndex = edge.To;
                    }
                }
                else // Not a straight away. Reset.
                {
                    if (currentStraightAwayIndex >= 0)
                        yield return (currentStraightAwayIndex, currentDirection, currentStraightAwayLength);
                    currentStraightAwayIndex = -1;
                    currentStraightAwayLength = 0;
                    currentDirection = Direction.None;
                    lastCellIndex = -1;
                }
            }
        }

        /// <summary>
        /// Lists the grid (row,column) tuple of each cell containing only a turn.
        /// </summary>
        /// <param name="maze">The maze to query.</param>
        /// <returns>An IEnumerable of Tuples containing the row and column.</returns>
        public static IEnumerable<(int Row, int Column)> Turns<N, E>(this Maze<N, E> maze)
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
        /// <returns>An IEnumerable of Tuples containing the row and column.</returns>
        public static IEnumerable<(int Row, int Column)> CrossSections<N, E>(this Maze<N, E> maze)
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
        /// <returns>An IEnumerable of Tuples containing the row and column.</returns>
        public static IEnumerable<(int Row, int Column)> MatchingExactly<N, E>(this Maze<N, E> maze, Direction directions)
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
        /// <returns>An IEnumerable of Tuples containing the row and column.</returns>
        public static IEnumerable<(int Row, int Column)> ContainsAll<N, E>(this Maze<N, E> maze, Direction directions)
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
        /// <returns>An IEnumerable of Tuples containing the row and column.</returns>
        public static IEnumerable<(int Row, int Column)> ContainsAny<N, E>(this Maze<N, E> maze, Direction directions)
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

        /// <summary>
        /// Query the maze to find all edges between cells where the directions do not match: Goes East but East neighbor does not go West.
        /// </summary>
        /// <param name="maze">The maze to query.</param>
        /// <returns>An IEnumerable of Tuples containing the cell index of the cell that has an extra direction and the inconsistent neighbor cell index.</returns>
        // Todo: This needs testing. Useful after say removing dead-ends inconsistently to determine edges that need a barrier or something.
        // Todo: Handle the Undefined flag.
        public static IEnumerable<(int cell1, int cell2)> FindInconsistentEdges<N, E>(this Maze<N, E> maze)
        {
            int width = maze.Width;
            int row, column;
            for (row = 1; row < maze.Height; row++)
            {
                for (column = 1; column < width; column++)
                {
                    if (!IsEastWestConsistentWithWesternCell(maze, column, row, out (int,int) eastWestEdge))
                    {
                        yield return eastWestEdge;
                    }

                    if (!IsNorthSouthConsistentWithSouthernCell(maze, column, row, out (int, int) northSouthEdge))
                    {
                        yield return northSouthEdge;
                    }
                }
            }
            row = 0;
            for (column = 1; column < width; column++)
            {
                if (!IsEastWestConsistentWithWesternCell(maze, column, row, out (int, int) eastWestEdge))
                {
                    yield return eastWestEdge;
                }
            }

            column = 0;
            for (row = 1; row < maze.Height; row++)
            {
                if (!IsNorthSouthConsistentWithSouthernCell(maze, column, row, out (int, int) northSouthEdge))
                {
                    yield return northSouthEdge;
                }
            }
        }

        static bool IsEastWestConsistentWithWesternCell<N, E>(Maze<N, E> maze, int column, int row, out (int,int) edge)
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

        static bool IsNorthSouthConsistentWithSouthernCell<N, E>(Maze<N, E> maze, int column, int row, out (int, int) edge)
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