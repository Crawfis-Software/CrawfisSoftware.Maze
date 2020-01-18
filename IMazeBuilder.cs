namespace CrawfisSoftware.Collections.Maze
{
    public interface IMazeBuilder<N, E>
    {
        int StartCell { get; }
        int EndCell { get; }
        System.Random RandomGenerator { get; set; }

        bool CarvePassage(int currentCell, int targetCell, bool preserveExistingCells = false);
        bool AddWall(int currentCell, int targetCell, bool preserveExistingCells = false);
        void BlockRegion(int lowerLeftCell, int upperRightCell);
        void OpenRegion(int lowerLeftCell, int upperRightCell);
        void CreateMaze(bool preserveExistingCells = false);
        Maze<N, E> GetMaze();
    }
}