using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawfisSoftware.Collections.Maze
{
    public abstract class MazeBuilderAbstract<N,E> : IMazeBuilder<N,E>
    {
        public int StartCell { get; set; }
        public int EndCell { get; set; }
        public System.Random RandomGenerator { get; set; }
        public MazeBuilderAbstract(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
        {
            this.width = width;
            this.height = height;
            nodeFunction = nodeAccessor;
            edgeFunction = edgeAccessor;
            grid = new Grid<N, E>(width, height, nodeAccessor, edgeAccessor);
            directions = new Direction[width, height];
            Clear();
            RandomGenerator = new Random();
        }

        public void CarvePassage(int currentCell, int targetCell)
        {
            int currentRow = currentCell / width;
            int currentColumn = currentCell % width;
            int selectedRow = targetCell / width;
            int selectedColumn = targetCell % width;
            Direction directionToNeighbor, directionToCurrent;
            if (grid.DirectionLookUp(currentCell, targetCell, out directionToNeighbor))
            {
                directions[currentColumn, currentRow] |= directionToNeighbor;
                if (grid.DirectionLookUp(targetCell, currentCell, out directionToCurrent))
                    directions[selectedColumn, selectedRow] |= directionToCurrent;
            }
        }

        public void AddWall(int currentCell, int targetCell)
        {
            int currentRow = currentCell / width;
            int currentColumn = currentCell % width;
            int selectedRow = targetCell / width;
            int selectedColumn = targetCell % width;
            Direction directionToNeighbor, directionToCurrent;
            if (grid.DirectionLookUp(currentCell, targetCell, out directionToNeighbor))
            {
                directions[currentColumn, currentRow] &= ~directionToNeighbor;
                if (grid.DirectionLookUp(targetCell, currentCell, out directionToCurrent))
                    directions[selectedColumn, selectedRow] &= ~directionToCurrent;
            }
        }

        public void BlockRegion(int lowerLeftCell, int upperRightCell)
        {
            int currentRow = lowerLeftCell / width;
            int currentColumn = lowerLeftCell % width;
            int endRow = upperRightCell / width;
            int endColumn = upperRightCell % width;
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
                for(int column = currentColumn; column < endColumn; column++)
                {
                    directions[column, row] = Direction.None;
                }
            }
        }


        public void OpenRegion(int lowerLeftCell, int upperRightCell)
        {
            int currentRow = lowerLeftCell / width;
            int currentColumn = lowerLeftCell % width;
            int endRow = upperRightCell / width;
            int endColumn = upperRightCell % width;
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
                    directions[column, row] = Direction.E | Direction.N | Direction.W | Direction.S;
                }
            }
        }
        private void Clear()
        {
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    directions[column, row] = Direction.Undefined;
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
        protected int width;
        protected int height;
        protected GetGridLabel<N> nodeFunction;
        protected GetEdgeLabel<E> edgeFunction;
        protected Direction[,] directions;
        #endregion
    }
}
