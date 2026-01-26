using CrawfisSoftware.Collections.Graph;

using System;
using System.Collections.Generic;

namespace CrawfisSoftware.Maze
{
    /// <summary>
    /// Implementation of IMazeBuilder to generate grid graphs or mazes.
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    public class MazeBuilder<N, E> : IMazeBuilder<N, E>
    {
        #region IMazeBuilder<N,E> Members
        /// <inheritdoc/>
        public int StartCell { get; set; }

        /// <inheritdoc/>
        public int EndCell { get; set; }

        /// <inheritdoc/>
        public System.Random RandomGenerator { get; set; }

        /// <inheritdoc/>
        public int Width { get; protected set; }

        /// <inheritdoc/>
        public int Height { get; protected set; }

        /// <inheritdoc/>
        public Grid<N, E> Grid { get { return grid; } }

        /// <inheritdoc/>
        public virtual Maze<N, E> GetMaze()
        {
            var maze = new Maze<N, E>(grid, directions.Directions, StartCell, EndCell);
            return maze;
        }

        /// <inheritdoc/>
        public Direction GetDirection(int column, int row)
        {
            return directions[column, row];
        }

        /// <inheritdoc/>
        public void SetCell(int i, int j, Direction dirs, bool preserveExistingCells = false)
        {
            bool cellsCanBeModified = !preserveExistingCells || ((directions[i, j] & Direction.Undefined) == Direction.Undefined);
            if (cellsCanBeModified)
                directions[i, j] = dirs;
        }

        /// <inheritdoc/>
        public bool CarvePassage(int currentColumn, int currentRow, int selectedColumn, int selectedRow, bool preserveExistingCells = false)
        {
            bool cellsCanBeModified = true;
            if (preserveExistingCells)
            {
                cellsCanBeModified = (directions[currentColumn, currentRow] & Direction.Undefined) == Direction.Undefined;
                cellsCanBeModified &= (directions[selectedColumn, selectedRow] & Direction.Undefined) == Direction.Undefined;
            }
            //Direction directionToNeighbor, directionToCurrent;
            if (cellsCanBeModified && grid.DirectionLookUp(currentColumn, currentRow, selectedColumn, selectedRow, out Direction directionToNeighbor))
            {
                directions[currentColumn, currentRow] |= directionToNeighbor;
                //directions[currentColumn, currentRow] &= ~Direction.Undefined;
                if (grid.DirectionLookUp(selectedColumn, selectedRow, currentColumn, currentRow, out Direction directionToCurrent))
                {
                    directions[selectedColumn, selectedRow] |= directionToCurrent;
                    //directions[selectedColumn, selectedRow] &= ~Direction.Undefined;
                }
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public bool AddWall(int currentColumn, int currentRow, int selectedColumn, int selectedRow, bool preserveExistingCells = false)
        {
            bool cellsCanBeModified = true;
            if (grid.DirectionLookUp(currentColumn, currentRow, selectedColumn, selectedRow, out Direction directionToNeighbor))
            {
                bool biDirectional = grid.DirectionLookUp(selectedColumn, selectedRow, currentColumn, currentRow, out Direction directionToCurrent);
                if (preserveExistingCells)
                {
                    cellsCanBeModified = (directions[currentColumn, currentRow] & Direction.Undefined) == Direction.Undefined;
                    if (biDirectional)
                        cellsCanBeModified &= (directions[selectedColumn, selectedRow] & Direction.Undefined) == Direction.Undefined;
                }
                if (cellsCanBeModified)
                {
                    directions[currentColumn, currentRow] &= ~directionToNeighbor;
                    if (biDirectional)
                    {
                        directions[selectedColumn, selectedRow] &= ~directionToCurrent;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    directions[column, row] = Direction.Undefined;
                }
            }
        }

        /// <inheritdoc/>
        public void RemoveUndefines()
        {
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    directions[column, row] &= ~Direction.Undefined;
                }
            }
        }

        /// <inheritdoc/>
        public void RemoveUndefine(int row, int column)
        {
            directions[column, row] &= ~Direction.Undefined;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void MakeBidirectionallyConsistent(int currentColumn, int currentRow, int endColumn, int endRow, bool carvingMissingPassages = true)
        {
            if (currentColumn < 0 || currentColumn >= grid.Width || endColumn < 0 || endColumn >= grid.Width)
            {
                throw new ArgumentOutOfRangeException("Specified cell is outside of the current maze width");
            }
            if (currentRow < 0 || currentRow >= grid.Height || endRow < 0 || endRow >= grid.Height)
            {
                throw new ArgumentOutOfRangeException("Specified cell is outside of the current maze height");
            }
            for (int row = currentRow; row <= endRow; row++)
            {
                for (int column = currentColumn; column <= endColumn; column++)
                {
                    Direction dir = directions[column, row];
                    if (column > 0 && (dir & Direction.W) == Direction.W)
                        if (carvingMissingPassages)
                        {
                            directions[column - 1, row] |= Direction.E;
                        }
                        else
                        {
                            directions[column, row] = dir & ~Direction.W;
                        }
                    if ((row < (Height - 1)) && (dir & Direction.N) == Direction.N)
                        if (carvingMissingPassages)
                        {
                            directions[column, row + 1] |= Direction.S;
                        }
                        else
                        {
                            directions[column, row] = dir & ~Direction.N;
                        }
                    if ((column < Width) && (dir & Direction.E) == Direction.E)
                        if (carvingMissingPassages)
                        {
                            directions[column + 1, row] |= Direction.W;
                        }
                        else
                        {
                            directions[column, row] = dir & ~Direction.E;
                        }
                    if ((row > 0) && (dir & Direction.S) == Direction.S)
                        if (carvingMissingPassages)
                        {
                            directions[column, row - 1] |= Direction.N;
                        }
                        else
                        {
                            directions[column, row] = dir & ~Direction.S;
                        }
                }
            }

        }

        /// <inheritdoc/>
        public void FreezeCellIfUndefined(int row, int column)
        {
            if (directions[column, row] != Direction.Undefined)
                RemoveUndefine(row, column);
        }

        /// <inheritdoc/>
        public void RemoveDirections(int i, int j, Direction dirs, bool preserveExistingCells = false)
        {
            Direction possibleDirection = directions[i, j] & ~dirs;
            SetCell(i, j, possibleDirection, preserveExistingCells);
        }

        /// <inheritdoc/>
        public void RemoveDirectionsExplicitly(int i, int j, Direction dirs)
        {
            directions[i, j] &= ~dirs;
        }

        /// <inheritdoc/>
        public void AddDirections(int i, int j, Direction dirs, bool preserveExistingCells = false)
        {
            Direction possibleDirection = directions[i, j] | dirs;
            SetCell(i, j, possibleDirection, preserveExistingCells);
        }
        /// <inheritdoc/>
        public void AddDirectionExplicitly(int i, int j, Direction dirs)
        {
            directions[i, j] |= dirs;
        }
        #endregion IMazeBuilder<N,E> Members


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">The width of the desired maze</param>
        /// <param name="height">The height of the desired maze</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        public MazeBuilder(int width, int height, GetGridLabel<N> nodeAccessor = null, GetEdgeLabel<E> edgeAccessor = null)
        {
            this.Width = width;
            this.Height = height;
            nodeFunction = nodeAccessor != null ? nodeAccessor : MazeBuilderUtility<N, E>.DummyNodeValues;
            edgeFunction = edgeAccessor != null ? edgeAccessor : MazeBuilderUtility<N, E>.DummyEdgeValues;
            grid = new Grid<N, E>(width, height, nodeFunction, edgeFunction);
            directions = new DirectionsInstrumented(width, height);
            Clear();
            RandomGenerator = new Random();
        }
        /// <summary>
        /// Copy Constructor (shallow copy)
        /// </summary>
        /// <param name="mazeBuilder">An existing maze builder to copy the current state from.</param>
        public MazeBuilder(MazeBuilder<N, E> mazeBuilder)
        {
            this.Width = mazeBuilder.Width;
            this.Height = mazeBuilder.Height;
            this.StartCell = mazeBuilder.StartCell;
            this.EndCell = mazeBuilder.EndCell;
            this.RandomGenerator = mazeBuilder.RandomGenerator;
            nodeFunction = mazeBuilder.nodeFunction;
            edgeFunction = mazeBuilder.edgeFunction;
            grid = mazeBuilder.grid;
            directions = mazeBuilder.directions;
            _cellChangedOrder = mazeBuilder._cellChangedOrder;
            _keepTrackOfChanges = mazeBuilder._keepTrackOfChanges;
        }


        /// <summary>
        /// Get the underlying Directions as a 2D array
        /// </summary>
        protected Direction[,] Directions
        {
            get { return directions.Directions; }
        }

        /// <summary>
        /// Get the order of cells that were changed during the maze creation process (if enabled).
        /// </summary>
        public IList<int> TraversalOrder { get { return _cellChangedOrder; } }

        /// <summary>
        /// Get or set whether to keep track of the order of cells that were changed during the maze creation process.
        /// </summary>
        public bool KeepTrackOfChanges
        {
            get { return _keepTrackOfChanges; }
            set
            {
                _keepTrackOfChanges = value;
                if (_keepTrackOfChanges)
                {
                    _cellChangedOrder = new List<int>(Width * Height);
                    directions.DirectionChanged += (row, column, oldValue, newValue) => _cellChangedOrder.Add(column + row * Width);
                }
                else
                {
                    _cellChangedOrder = null;
                }
            }
        }

        #region Member variables
        /// <summary>
        /// The underlying grid data structure
        /// </summary>
        protected Grid<N, E> grid;
        /// <summary>
        /// A function used to look up node labels
        /// </summary>
        protected GetGridLabel<N> nodeFunction;
        /// <summary>
        /// A function used to look up edge weights
        /// </summary>
        protected GetEdgeLabel<E> edgeFunction;
        private bool _keepTrackOfChanges;

        /// <summary>
        /// A 2D array storing the structure of the maze as a 2D array of directions
        /// </summary>
        private DirectionsInstrumented directions;
        private List<int> _cellChangedOrder;
        #endregion
    }
}
