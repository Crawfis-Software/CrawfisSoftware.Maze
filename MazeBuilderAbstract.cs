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
        protected void CarvePassage(int currentCell, int targetCell)
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

        public abstract Maze<N, E> GetMaze();

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
