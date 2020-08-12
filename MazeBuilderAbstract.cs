using CrawfisSoftware.Collections.Graph;
using System;

namespace CrawfisSoftware.Collections.Maze
{
    public abstract class MazeBuilderAbstract<N, E> : IMazeBuilder<N, E>
    {
        public int StartCell { get; set; }
        public int EndCell { get; set; }
        public System.Random RandomGenerator { get; set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }

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

        public void CarveDirectionally(int currentColumn, int currentRow, Direction directionsToCarve)
        {
            directions[currentColumn, currentRow] |= directionsToCarve;
        }

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
                if (grid.DirectionLookUp(selectedColumn, selectedRow, currentColumn, currentRow, out Direction directionToCurrent))
                {
                    directions[selectedColumn, selectedRow] |= directionToCurrent;
                    return true;
                }
            }
            return false;
        }
        public bool CarvePassage(int currentCell, int targetCell, bool preserveExistingCells = false)
        {
            int currentRow = currentCell / Width;
            int currentColumn = currentCell % Width;
            int selectedRow = targetCell / Width;
            int selectedColumn = targetCell % Width;
            return CarvePassage(currentColumn, currentRow, selectedColumn, selectedRow, preserveExistingCells);
        }

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

        public void BlockRegion(int lowerLeftCell, int upperRightCell)
        {
            FillRegion(lowerLeftCell, upperRightCell, Direction.None);
        }


        public void OpenRegion(int lowerLeftCell, int upperRightCell)
        {
            FillRegion(lowerLeftCell, upperRightCell, Direction.E | Direction.N | Direction.W | Direction.S);
        }

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

        public void MakeConsistent()
        {
            MakeConsistent(Width + 1, Width * Height - 1 - Width - 1);
        }
        /// <summary>
        /// If a Neighbor edge does not match and has Undefined as a flag, set the edge to match.
        /// </summary>
        public void MakeConsistent(int lowerLeftCell, int upperRightCell)
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

        public abstract void CreateMaze(bool preserveExistingCells);
        public virtual Maze<N, E> GetMaze()
        {
            return new Maze<N, E>(grid, directions);
        }

        #region Member variables
        protected Grid<N, E> grid;
        protected GetGridLabel<N> nodeFunction;
        protected GetEdgeLabel<E> edgeFunction;
        protected Direction[,] directions;
        #endregion
    }
}
