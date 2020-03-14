using CrawfisSoftware.Collections.Graph;

namespace CrawfisSoftware.Collections.Maze
{
    public class MazeBuilderExplicit<N, E> : MazeBuilderAbstract<N, E>
    {
        public MazeBuilderExplicit(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
            : base(width, height, nodeAccessor, edgeAccessor)
        {
        }
        public void SetCell(int i, int j, Direction dirs)
        {
            directions[i, j] = dirs;
        }
        public override void CreateMaze(bool preserveExistingCells)
        {
        }
    }
}
