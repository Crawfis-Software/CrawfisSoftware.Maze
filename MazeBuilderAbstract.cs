using CrawfisSoftware.Collections.Graph;
using System;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Abstract base class to build mazes with some concrete implemetations
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
            nodeFunction = nodeAccessor != null ? nodeAccessor : MazeBuilderUtility<N,E>.DummyNodeValues;
            edgeFunction = edgeAccessor != null ? edgeAccessor : MazeBuilderUtility<N, E>.DummyEdgeValues;
            grid = new Grid<N, E>(width, height, nodeFunction, edgeFunction);
            directions = new Direction[width, height];
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
            nodeFunction = mazeBuilder.nodeFunction;
            edgeFunction = mazeBuilder.edgeFunction;
            grid = mazeBuilder.grid;
            directions = mazeBuilder.directions;
            RandomGenerator = mazeBuilder.RandomGenerator;
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

        /// <summary>
        /// Remove all directions in the specified region.
        /// </summary>
        /// <param name="lowerLeftCell">The lower-left corner of the region to fix.</param>
        /// <param name="upperRightCell">The upper-right corner of the region to fix.</param>
        /// <remarks>May lead to possible inconsistent neighbor directions.</remarks>
        /// <seealso>MakeBidirectionallyConsistent</seealso>
        public void BlockRegion(int lowerLeftCell, int upperRightCell)
        {
            FillRegion(lowerLeftCell, upperRightCell, Direction.None);
        }

        /// <summary>
        /// Add passages to all neighbors within the specified region
        /// </summary>
        /// <param name="lowerLeftCell">The lower-left corner of the region to fix.</param>
        /// <param name="upperRightCell">The upper-right corner of the region to fix.</param>
        /// <remarks>May lead to possible inconsistent neighbor directions.</remarks>
        /// <seealso>MakeBidirectionallyConsistent</seealso>
        public void OpenRegion(int lowerLeftCell, int upperRightCell)
        {
            FillRegion(lowerLeftCell, upperRightCell, Direction.E | Direction.N | Direction.W | Direction.S);
        }

        /// <inheritdoc/>
        public void FillRegion(int lowerLeftCell, int upperRightCell, Direction dirs)
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
            for (int row = currentRow; row < endRow; row++)
            {
                for (int column = currentColumn; column < endColumn; column++)
                {
                    directions[column, row] = dirs;
                }
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    directions[column, row] |= Direction.Undefined;
                }
            }
        }

        /// <inheritdoc/>
        public void RemoveDeadEnds(bool preserveExistingCells = false)
        {
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    Direction dir = directions[column, row] & ~Direction.Undefined;
                    switch (dir)
                    {
                        case Direction.W:
                            AddWall(column, row, column - 1, row, preserveExistingCells);
                            break;
                        case Direction.N:
                            AddWall(column, row, column, row + 1, preserveExistingCells);
                            break;
                        case Direction.E:
                            AddWall(column, row, column + 1, row, preserveExistingCells);
                            break;
                        case Direction.S:
                            AddWall(column, row, column, row - 1, preserveExistingCells);
                            break;
                    }
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
        public void FreezeCells()
        {
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    if (directions[column, row] != Direction.Undefined)
                        directions[column, row] &= ~Direction.Undefined;
                }
            }
        }

        /// <inheritdoc/>
        public void MakeBidirectionallyConsistent()
        {
            MakeBiDirectionallyConsistent(Width + 1, Width * Height - 1 - Width - 1);
        }

        /// <inheritdoc/>
        public void MakeBiDirectionallyConsistent(int lowerLeftCell, int upperRightCell)
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
            for (int row = currentRow; row < endRow; row++)
            {
                for (int column = currentColumn; column < endColumn; column++)
                {
                    Direction dir = directions[column, row];
                    if ((dir & Direction.W) == Direction.W)
                        directions[column - 1, row] |= Direction.E;
                    else
                        directions[column - 1, row] &= ~Direction.E;
                    if ((dir & Direction.N) == Direction.N)
                        directions[column, row + 1] |= Direction.N;
                    else
                        directions[column, row + 1] &= ~Direction.N;
                    if ((dir & Direction.E) == Direction.E)
                        directions[column + 1, row] |= Direction.W;
                    else
                        directions[column + 1, row] &= ~Direction.W;
                    if ((dir & Direction.S) == Direction.S)
                        directions[column, row - 1] |= Direction.N;
                    else
                        directions[column, row - 1] &= ~Direction.N;
                }
            }

        }

        /// <inheritdoc/>
        public abstract void CreateMaze(bool preserveExistingCells = false);

        /// <inheritdoc/>
        public virtual Maze<N, E> GetMaze()
        {
            var maze = new Maze<N, E>(grid, directions, StartCell, EndCell);
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
        public void SetCell(int i, int j, Direction dirs)
        {
            directions[i, j] = dirs;
        }

        /// <inheritdoc/>
        public void AddDirectionExplicitly(int i, int j, Direction dirs)
        {
            directions[i, j] |= dirs;
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
        /// <summary>
        /// A 2D array storing the structure of the maze as a 2D array of directions
        /// </summary>
        protected Direction[,] directions;
        #endregion
    }
}
