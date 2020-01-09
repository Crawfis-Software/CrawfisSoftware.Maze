namespace CrawfisSoftware.Collections.Maze
{
    public interface IMazeBuilder<N, E>
    {
        int StartCell { get; }
        int EndCell { get; }
        System.Random RandomGenerator { get; set; }

        void CarvePassage(int currentCell, int targetCell);
        void AddWall(int currentCell, int targetCell);
        void BlockRegion(int lowerLeftCell, int upperRightCell);
        void OpenRegion(int lowerLeftCell, int upperRightCell);
        void CreateMaze(bool preserveExistingCells = false);
        Maze<N, E> GetMaze();
    }
}