using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawfisSoftware.Collections.Maze
{
    public abstract class MazeBuilderAbstract<N> : IMazeBuilder<N>
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

        public abstract Maze<N> GetMaze();

        #region Member variables
        protected Grid<N, int> grid;
        protected int width;
        protected int height;
        protected GetGridLabel<N> nodeFunction;
        protected GetEdgeLabel<int> edgeFunction;
        protected Direction[,] directions;
        #endregion
    }
}
