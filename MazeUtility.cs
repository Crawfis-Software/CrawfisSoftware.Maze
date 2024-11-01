using CrawfisSoftware.Collections.Graph;

using System;
using System.Collections.Generic;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Static methods extending the Maze class.
    /// </summary>
    public static class MazeUtility
    {
        /// <summary>
        /// Given a maze, reverses its directions. Undefined is handled separately.
        /// </summary>
        /// <typeparam name="N">The type used for node labels</typeparam>
        /// <typeparam name="E">The type used for edge weights</typeparam>
        /// <param name="maze">The maze to invert.</param>
        /// <param name="removeUndefines">If false, Direction.Undefined is preserved.If true, Direction.Undefined is ignored and stripped.</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        /// <returns>A maze</returns>
        /// <remarks>The NodeAccessor and EdgeAccessor's are not preserved by default.</remarks>
        public static Maze<N, E> Inverse<N, E>(this Maze<N, E> maze, bool removeUndefines = false, GetGridLabel<N> nodeAccessor = null, GetEdgeLabel<E> edgeAccessor = null)
        {
            var mazeBuilder = new MazeBuilderExplicit<N, E>(maze.Width, maze.Height);
            Direction allDirections = Direction.None;
            //foreach (Direction dir in Enum.GetValues<Direction>()) allDirections |= dir; // Generic version not available in .Net Standard 2.1
            foreach (var dir in Enum.GetValues(typeof(Direction))) allDirections |= (Direction)dir;
            // Masks out Undefined.
            allDirections &= ~Direction.Undefined;
            for (int row = 0; row < maze.Height; row++)
            {
                for (int column = 0; column < maze.Width; column++)
                {
                    Direction directions = maze.GetDirection(column, row);
                    Direction isUndefined = directions & Direction.Undefined;
                    Direction inverseDirection = allDirections & ~directions;
                    if (!removeUndefines)
                        inverseDirection |= isUndefined;
                    mazeBuilder.SetCell(column, row, inverseDirection);
                }
            }
            return mazeBuilder.GetMaze();
        }
    }
}