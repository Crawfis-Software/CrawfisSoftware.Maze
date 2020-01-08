namespace CrawfisSoftware.Collections.Maze
{
    public interface IMazeBuilder<N, E>
    {
        Maze<N, E> GetMaze();
    }
}