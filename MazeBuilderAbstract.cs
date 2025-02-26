using CrawfisSoftware.Collections;
using CrawfisSoftware.Collections.Graph;
using CrawfisSoftware.Maze;

using System;
using System.Collections.Generic;
using System.Linq;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Abstract base class to build mazes with some concrete implementations
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    public abstract class MazeBuilderAbstract<N, E> : IMazeBuilder<N, E>
    {
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

        /// <summary>
        /// Get the underlying grid data structure
        /// </summary>
        public Grid<N, E> Grid { get { return grid; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">The width of the desired maze</param>
        /// <param name="height">The height of the desired maze</param>
        /// <param name="nodeAccessor">A function to retrieve any node labels</param>
        /// <param name="edgeAccessor">A function to retrieve any edge weights</param>
        public MazeBuilderAbstract(int width, int height, GetGridLabel<N> nodeAccessor = null, GetEdgeLabel<E> edgeAccessor = null)
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
        public MazeBuilderAbstract(MazeBuilderAbstract<N, E> mazeBuilder)
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
        /// Get the direction for the specified cell.
        /// </summary>
        /// <param name="column">The column index of the cell.</param>
        /// <param name="row">The row index of the cell.</param>
        /// <returns>The Direction flags.</returns>
        public Direction GetDirection(int column, int row)
        {
            return directions[column, row];
        }

        /// <inheritdoc/>
        public bool CarveDirectionally(int currentColumn, int currentRow, Direction directionToCarve, bool preserveExistingCells = false)
        {
            int cellIndex = currentRow * Width + currentColumn;
            switch (directionToCarve)
            {
                case Direction.None:
                    break;
                case Direction.W:
                    return CarvePassage(cellIndex, cellIndex - 1, preserveExistingCells);
                case Direction.N:
                    return CarvePassage(cellIndex, cellIndex + Width, preserveExistingCells);
                case Direction.E:
                    return CarvePassage(cellIndex, cellIndex + 1, preserveExistingCells);
                case Direction.S:
                    return CarvePassage(cellIndex, cellIndex - Width, preserveExistingCells);
            }
            return false;
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
        public bool CarvePassage(int currentCell, int targetCell, bool preserveExistingCells = false)
        {
            int currentRow = currentCell / Width;
            int currentColumn = currentCell % Width;
            int selectedRow = targetCell / Width;
            int selectedColumn = targetCell % Width;
            return CarvePassage(currentColumn, currentRow, selectedColumn, selectedRow, preserveExistingCells);
        }

        /// <inheritdoc/>
        public bool AddWall(int currentCell, int targetCell, bool preserveExistingCells = false)
        {
            int currentRow = currentCell / Width;
            int currentColumn = currentCell % Width;
            int selectedRow = targetCell / Width;
            int selectedColumn = targetCell % Width;
            return AddWall(currentColumn, currentRow, selectedColumn, selectedRow, preserveExistingCells);
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
        public void BlockRegion(int lowerLeftCell, int upperRightCell, bool preserveExistingCells = false)
        {
            FillRegion(lowerLeftCell, upperRightCell, Direction.None, preserveExistingCells);
        }

        /// <inheritdoc/>
        public void OpenRegion(int lowerLeftCell, int upperRightCell, bool preserveExistingCells = false, bool markAsUndefined = true)
        {
            Direction dirs = Direction.N | Direction.E | Direction.S | Direction.W;
            if(markAsUndefined) dirs |= Direction.Undefined;
            FillRegion(lowerLeftCell, upperRightCell, dirs, preserveExistingCells);
        }

        /// <inheritdoc/>
        public void FillRegion(int lowerLeftCell, int upperRightCell, Direction dirs, bool preserveExistingCells = false)
        {
            int currentRow = lowerLeftCell / Width;
            int currentColumn = lowerLeftCell % Width;
            int endRow = upperRightCell / Width;
            int endColumn = upperRightCell % Width;
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
                    SetCell(column, row, dirs, preserveExistingCells);
                    //directions[column, row] = dirs;
                }
            }
        }

        /// <summary>
        /// Add walls (inconsistently currently) to the boundary of the define rectangle.
        /// </summary>
        /// <param name="lowerLeftCell">The lower-left cell index.</param>
        /// <param name="upperRightCell">The upper -right cell index.</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        public void WallBoundary(bool preserveExistingCells = false)
        {
            int lowerLeftCell = 0;
            int upperRightCell = Width * Height - 1;
            int lowerRightCell = upperRightCell % Width + Width * (int)(lowerLeftCell / Width);
            int upperLeftCell = lowerLeftCell % Width + Width * (int)(upperRightCell / Width);
            int row = lowerLeftCell / Width;
            int col;
            for (col = lowerLeftCell % Width; col <= upperRightCell % Width; col++)
            {
                RemoveDirections(col, row, ~Direction.S, preserveExistingCells);
            }
            row = upperRightCell / Width;
            for (col = lowerLeftCell % Width; col <= upperRightCell % Width; col++)
            {
                RemoveDirections(col, row, ~Direction.N, preserveExistingCells);
            }
            col = lowerLeftCell % Width;
            for (row = lowerLeftCell / Width; row <= upperRightCell / Width; row++)
            {
                RemoveDirections(col, row, ~Direction.W, preserveExistingCells);
            }
            col = upperRightCell % Width;
            for (row = lowerLeftCell / Width; row <= upperRightCell / Width; row++)
            {
                RemoveDirections(col, row, ~Direction.E, preserveExistingCells);
            }
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
        public void RemoveDeadEnds(int maxCount = int.MaxValue, bool preserveExistingCells = false)
        {
            var deadEnds = new List<(int col1, int row1, int col2, int row2)>();
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    Direction dir = directions[column, row] & ~Direction.Undefined;
                    switch (dir)
                    {
                        case Direction.W:
                            //AddWall(column, row, column - 1, row, preserveExistingCells);
                            deadEnds.Add((column, row, column - 1, row));
                            break;
                        case Direction.N:
                            //AddWall(column, row, column, row + 1, preserveExistingCells);
                            deadEnds.Add((column, row, column, row + 1));
                            break;
                        case Direction.E:
                            //AddWall(column, row, column + 1, row, preserveExistingCells);
                            deadEnds.Add((column, row, column + 1, row));
                            break;
                        case Direction.S:
                            //AddWall(column, row, column, row - 1, preserveExistingCells);
                            deadEnds.Add((column, row, column, row - 1));
                            break;
                    }
                }
                int count = 0;
                foreach (var deadEnd in deadEnds)
                {
                    AddWall(deadEnd.col1, deadEnd.row1, deadEnd.col2, deadEnd.row2, preserveExistingCells);
                    count++;
                    if (count >= maxCount)
                        break;
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
        public void FreezeDefinedCells()
        {
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    FreezeCellIfUndefined(row, column);
                }
            }
        }

        /// <summary>
        /// Remove the Undefined (freeze) the specified cell if it has a Direction set.
        /// </summary>
        /// <param name="row">A row index</param>
        /// <param name="column">A column index</param>
        public void FreezeCellIfUndefined(int row, int column)
        {
            if (directions[column, row] != Direction.Undefined)
                RemoveUndefine(row, column);
        }

        /// <summary>
        /// Remove the Undefined flag from the specified cell.
        /// </summary>
        /// <param name="row">A row index</param>
        /// <param name="column">A column index</param>
        public void RemoveUndefine(int row, int column)
        {
            directions[column, row] &= ~Direction.Undefined;
        }

        /// <inheritdoc/>
        public void MakeBidirectionallyConsistent(bool carvingMissingPassages = true)
        {
            MakeBidirectionallyConsistent(0, 0, Width - 1, Height - 1);
        }

        /// <inheritdoc/>
        public void MakeBidirectionallyConsistent(int lowerLeftCell, int upperRightCell, bool carvingMissingPassages = true)
        {
            int currentRow = lowerLeftCell / Width;
            int currentColumn = lowerLeftCell % Width;
            int endRow = upperRightCell / Width;
            int endColumn = upperRightCell % Width;
            MakeBidirectionallyConsistent(currentColumn, currentRow, endColumn, endRow);
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
                    //else if (column > 0)
                    //    directions[column - 1, row] &= ~Direction.E;
                    if ((row < (Height - 1)) && (dir & Direction.N) == Direction.N)
                        if (carvingMissingPassages)
                        {
                            directions[column, row + 1] |= Direction.S;
                        }
                        else
                        {
                            directions[column, row] = dir & ~Direction.N;
                        }
                    //else if (row < Height)
                    //    directions[column, row + 1] &= ~Direction.S;
                    if ((column < Width) && (dir & Direction.E) == Direction.E)
                        if (carvingMissingPassages)
                        {
                            directions[column + 1, row] |= Direction.W;
                        }
                        else
                        {
                            directions[column, row] = dir & ~Direction.E;
                        }
                    //else if(column < Width)
                    //    directions[column + 1, row] &= ~Direction.W;
                    if ((row > 0) && (dir & Direction.S) == Direction.S)
                        if (carvingMissingPassages)
                        {
                            directions[column, row - 1] |= Direction.N;
                        }
                        else
                        {
                            directions[column, row] = dir & ~Direction.S;
                        }
                    //else if(row > 0)
                    //    directions[column, row - 1] &= ~Direction.N;
                }
            }

        }

        /// <summary>
        /// Invert all of the directions, keeping Undefine's unchanged.
        /// </summary>
        public void InvertDirections()
        {
            Direction allDirections = Direction.None;
            //foreach (Direction dir in Enum.GetValues<Direction>()) allDirections |= dir; // Generic version not available in .Net Standard 2.1
            foreach (var dir in Enum.GetValues(typeof(Direction))) allDirections |= (Direction)dir;
            // Masks out Undefined.
            allDirections &= ~Direction.Undefined;
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    Direction cellDirections = directions[column, row];
                    Direction isUndefined = cellDirections & Direction.Undefined;
                    Direction inverseDirection = allDirections & ~cellDirections;
                    // Add Undefine back in if it was set.
                    inverseDirection |= isUndefined;
                    directions[column, row] = inverseDirection;
                }
            }

        }

        /// <inheritdoc/>
        public abstract void CreateMaze(bool preserveExistingCells = false);

        /// <inheritdoc/>
        public virtual Maze<N, E> GetMaze()
        {
            var maze = new Maze<N, E>(grid, directions.Directions, StartCell, EndCell);
            return maze;
        }

        /// <inheritdoc/>
        public void CarveHorizontalSpan(int row, int column1, int column2, bool preserveExistingCells)
        {
            int start = column2;
            int end = column1;
            column1 = column2;
            if (start > end)
            {
                start = end;
                end = column2;
            }
            for (int i = start; i < end; i++)
            {
                CarveDirectionally(i, row, Direction.E, preserveExistingCells);
            }
        }

        /// <inheritdoc/>
        public void CarveVerticalSpan(int column, int row1, int row2, bool preserveExistingCells)
        {
            int start = row1;
            int end = row2;
            if (start > end)
            {
                start = end;
                end = row1;
            }
            for (int i = start; i < end; i++)
            {
                CarveDirectionally(column, i, Direction.N, preserveExistingCells);
            }
        }

        /// <inheritdoc/>
        public void SetCell(int i, int j, Direction dirs, bool preserveExistingCells = false)
        {
            bool cellsCanBeModified = !preserveExistingCells || ((directions[i, j] & Direction.Undefined) == Direction.Undefined);
            if (cellsCanBeModified)
                directions[i, j] = dirs;
        }

        public void RemoveDirections(int i, int j, Direction dirs, bool preserveExistingCells = false)
        {
            Direction possibleDirection = directions[i, j] & ~dirs;
            SetCell(i, j, possibleDirection, preserveExistingCells);
        }
        public void RemoveDirectionsExplicitly(int i, int j, Direction dirs)
        {
            directions[i, j] &= ~dirs;
        }

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

        /// <summary>
        /// Get the underlying Directions as a 2D array
        /// </summary>
        protected Direction[,] Directions
        {
            get { return directions.Directions; }
        }

        /// <summary>
        /// Swap out the underlying data storage with the new set of Directions.
        /// </summary>
        /// <param name="newDirections">A 2D array of Directions (which should be the same size as the original)</param>
        protected void ReplaceDirections(Direction[,] newDirections)
        {
            directions.ReplaceDirections(newDirections);
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
