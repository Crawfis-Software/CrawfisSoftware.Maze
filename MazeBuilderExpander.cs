using CrawfisSoftware.Collections.Graph;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Expand an existing MazeBuilder to have wider opening, and/or wider walls, and/or a border
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    public class MazeBuilderExpander<N, E> : MazeBuilderAbstract<N, E>
    {
        private int _numberOfWallTiles = 0;
        private int _numberOfOpeningTiles = 0;
        private int _numberOfBorderTiles = 0;
        private MazeBuilderAbstract<N, E> _originalMazeBuilder;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mazeBuilder">A previous MazeBuilder</param>
        public MazeBuilderExpander(MazeBuilderAbstract<N, E> mazeBuilder) : base(mazeBuilder)
        {
            _originalMazeBuilder = mazeBuilder;
        }

        /// <summary>
        /// Set the size of the wall thickness in terms of the number of cells The default wall size is 0.
        /// </summary>
        /// <param name="numberOfTilesToExpandBy">The number of cells to expand each wall to.</param>
        public void ExpandWalls(int numberOfTilesToExpandBy)
        {
            if(numberOfTilesToExpandBy < 0)
            {
                throw new System.ArgumentOutOfRangeException("Wall expansion needs to be zero or positive.");
            }
            _numberOfOpeningTiles = numberOfTilesToExpandBy;
        }


        /// <summary>
        /// Increase the opening thickness in terms of the number of cells. The default opening size is 1.
        /// </summary>
        /// <param name="numberOfTilesToExpandBy">The number of cells to expand each opening to.</param>
        public void ExpandOpenings(int numberOfTilesToExpandBy)
        {
            if (numberOfTilesToExpandBy < 0)
            {
                throw new System.ArgumentOutOfRangeException("Wall expansion needs to be zero or positive.");
            }
            _numberOfWallTiles = numberOfTilesToExpandBy;
        }


        /// <summary>
        /// Set the size of a border on all sides in terms of the number of cells (default is 0).
        /// </summary>
        /// <param name="numberOfTilesToExpandBy">The number of cells for the border.</param>
        /// <remarks>Note: The Start and End cells will be set to the interior of the maze corresponding to the mapped cell location previously. 
        /// Use one of the path carving algorithms to create an exit out of the boundary.</remarks>
        public void AddBorder(int numberOfTilesToExpandBy)
        {
            if (numberOfTilesToExpandBy < 0)
            {
                throw new System.ArgumentOutOfRangeException("Wall expansion needs to be zero or positive.");
            }
            _numberOfBorderTiles = numberOfTilesToExpandBy;
        }

        /// <inheritdoc/>
        public override void CreateMaze(bool preserveExistingCells = false)
        {
            int startColumn = StartCell % Width;
            int startRow = StartCell / Width;
            int endColumn = EndCell % Width;
            int endRow = EndCell / Width;
            this.StartCell = _numberOfBorderTiles + startColumn * (_numberOfOpeningTiles + _numberOfWallTiles) + _numberOfOpeningTiles / 2
                + _numberOfBorderTiles + Width * startRow * (_numberOfOpeningTiles + _numberOfWallTiles) + Width * _numberOfOpeningTiles / 2;
            this.EndCell = _numberOfBorderTiles + endColumn * (_numberOfOpeningTiles + _numberOfWallTiles) + _numberOfOpeningTiles / 2
                + _numberOfBorderTiles + Width * endRow * (_numberOfOpeningTiles + _numberOfWallTiles) + Width * _numberOfOpeningTiles / 2;
            Width = Width + Width * _numberOfOpeningTiles + Width * _numberOfWallTiles + 2 * _numberOfBorderTiles;
            Height = Height + Height * _numberOfOpeningTiles + Height * _numberOfWallTiles + 2 * _numberOfBorderTiles;
            grid = new Grid<N, E>(Width, Height, nodeFunction, edgeFunction);
            directions = ExpandDirections(directions);
        }

        private Direction[,] ExpandDirections(Direction[,] directions)
        {
            Direction[,] newDirections = new Direction[Width, Height];
            for(int row = _numberOfBorderTiles; row < Height-_numberOfBorderTiles; row++)
            {
                int expansionSize = (1 + _numberOfOpeningTiles + _numberOfWallTiles);
                int oldRow = (row - _numberOfBorderTiles) / expansionSize;
                for(int column = _numberOfBorderTiles; column < Width-_numberOfBorderTiles; column++)
                {
                    newDirections[column, row] = Direction.None;
                    int oldColumn = (column - _numberOfBorderTiles) / expansionSize;
                    Direction direction = directions[oldColumn, oldRow];
                    if ((direction | Direction.Undefined) == (Direction.None | Direction.Undefined)) continue;
                    if ((column - _numberOfBorderTiles) % expansionSize == 0)
                    {
                        if ((row - _numberOfBorderTiles) % expansionSize == 0)
                        {
                            newDirections[column, row] = direction;
                        }
                        else if ((direction & Direction.N) == Direction.N)
                        {
                            newDirections[column, row] = Direction.N | Direction.S;
                        }
                    }
                    else if((row - _numberOfBorderTiles) % expansionSize == 0 && ((direction & Direction.E) == Direction.E))
                    {
                        newDirections[column, row] = Direction.E | Direction.W;
                    }
                }
            }
            
            return newDirections;
        }
    }
}