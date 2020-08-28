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
        public MazeBuilderAbstract(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
        {
            this.Width = width;
            this.Height = height;
            nodeFunction = nodeAccessor;
            edgeFunction = edgeAccessor;
            grid = new Grid<N, E>(width, height, nodeAccessor, edgeAccessor);
            directions = new Direction[width, height];
            Clear();
            RandomGenerator = new Random();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mazeBuilder">An existing maze builder to copy the current state from.</param>
        public MazeBuilderAbstract(MazeBuilderAbstract<N,E> mazeBuilder)
        {
            this.Width = mazeBuilder.Width;
            this.Height = mazeBuilder.Height;
            nodeFunction = mazeBuilder.nodeFunction;
            edgeFunction = mazeBuilder.edgeFunction;
            grid = mazeBuilder.grid;
            directions = mazeBuilder.directions;
            RandomGenerator = mazeBuilder.RandomGenerator;
        }

        /// <summary>
        /// Carve a passage in the specified direction.
        /// </summary>
        /// <param name="currentColumn">Column index of the cell to carve</param>
        /// <param name="currentRow">Row index of the row to carve</param>
        /// <param name="directionToCarve">A single direction to carve</param>
        /// <param name="preserveExistingCells">Boolean indicating whether to only replace maze cells that are undefined.
        /// Default is false.</param>
        /// <return>Returns true if the operation was successful.</return>
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
            return AddWall(currentRow, currentColumn, selectedRow, selectedColumn, preserveExistingCells);
        }

        private bool AddWall(int currentRow, int currentColumn, int selectedRow, int selectedColumn, bool preserveExistingCells = false)
        {
            bool cellsCanBeModified = true;
            if (preserveExistingCells)
            {
                cellsCanBeModified = (directions[currentColumn, currentRow] & Direction.Undefined) == Direction.Undefined;
                cellsCanBeModified &= (directions[selectedColumn, selectedRow] & Direction.Undefined) == Direction.Undefined;
            }
            if (cellsCanBeModified && grid.DirectionLookUp(currentColumn, currentRow, selectedColumn, selectedRow, out Direction directionToNeighbor))
            {
                directions[currentColumn, currentRow] &= ~directionToNeighbor;
                if (grid.DirectionLookUp(currentColumn, currentRow, selectedColumn, selectedRow, out Direction directionToCurrent))
                {
                    directions[selectedColumn, selectedRow] &= ~directionToCurrent;
                    return true;
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

        /// <summary>
        /// Set all cells with the directions specified.
        /// </summary>
        /// <param name="lowerLeftCell">The lower-left corner of the region to fix.</param>
        /// <param name="upperRightCell">The upper-right corner of the region to fix.</param>
        /// <param name="dirs">List of directions to set each cell to.</param>
        /// <remarks>May lead to possible inconsistent neighbor directions.</remarks>
        /// <seealso>MakeBidirectionallyConsistent</seealso>
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

        /// <summary>
        /// Set all directions in the maze to Direction.Undefined
        /// </summary>
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

        /// <summary>
        /// Remove Direction.Undefined for all cells.
        /// </summary>
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

        /// <summary>
        /// Remove Direction.Undefined for all cells that have been defined
        /// </summary>
        public void FreezeCells()
        {
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    if(directions[column,row] != Direction.Undefined)
                        directions[column, row] &= ~Direction.Undefined;
                }
            }
        }

        /// <summary>
        /// Ensures that all edges are bi-directional. In other words, a passage was not carved from A to
        /// B and not B to A.
        /// </summary>
        /// <remarks>This will open up all inconsistencies.</remarks>
        public void MakeBidirectionallyConsistent()
        {
            MakeBiDirectionallyConsistent(Width + 1, Width * Height - 1 - Width - 1);
        }

        /// <summary>
        /// Ensures that all edges are bi-directional. In other words, a passage was not carved from A to
        /// B and not B to A.
        /// </summary>
        /// <param name="lowerLeftCell">The lower-left corner of the region to fix.</param>
        /// <param name="upperRightCell">The upper-right corner of the region to fix.</param>
        /// <remarks>This will open up all inconsistencies.</remarks>
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
            return new Maze<N, E>(grid, directions);
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
