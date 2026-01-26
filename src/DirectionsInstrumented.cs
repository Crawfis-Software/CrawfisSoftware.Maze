using CrawfisSoftware.Collections;
using CrawfisSoftware.Collections.Graph;

using System;

namespace CrawfisSoftware.Maze
{
    /// <summary>
    /// A 2D array with an event to notify you every time the array is possibly changed.
    /// </summary>
    public class DirectionsInstrumented : Instrumented2DArray<Direction>
    {
        /// <summary>
        /// A renamed event to indicate that a direction in the array has changed.
        /// </summary>
        /// <remarks>Same as ValueChanged event.</remarks>
        public event Action<int, int, Direction, Direction> DirectionChanged
        {
            add { ValueChanged += value; }
            remove { ValueChanged -= value; }
        }

        /// <summary>
        /// Get a 2D array of Directions.
        /// </summary>
        public Direction[,] Directions
        {
            get { return GetArray(); }
        }

        /// <inheritdoc/>
        public DirectionsInstrumented(int width, int height) : base(width, height)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="directions">A 2D array of Direction's.</param>
        public DirectionsInstrumented(Direction[,] directions) : base(directions)
        {
        }
        /// <summary>
        /// Replace the underlying data with a new 2D array of Direction's. Shallow copy.
        /// </summary>
        /// <param name="newDirections">An array of type T.</param>
        public void ReplaceDirections(Direction[,] newDirections)
        {
            ReplaceArray(newDirections);
        }
    }
}