using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawfisSoftware.Collections.Graph;
using CrawfisSoftware.Collections.Maze;

namespace CrawfisSoftware.Collections.Maze
{
    public class MazeBuilderRandomWalk<N,E> : MazeBuilderAbstract<N,E>
    {
        // Having Walker in a seperate class allows for multiple walkers. Need to make this thread safe though.
        private class Walker
        {
            private MazeBuilderRandomWalk<N,E> mazeBuilder;
            public int currentCell;
            private System.Random random;
            private bool preserveExistingCells;
            private bool favorForwardCarving;
            private int[] nextCellIncrement;
            private int lastMove;
            public void StartWalker(MazeBuilderRandomWalk<N,E> mazeBuilder, int cell, bool preserveExistingCells, bool favorForwardCarving, System.Random random)
            {
                this.mazeBuilder = mazeBuilder;
                currentCell = cell;
                this.preserveExistingCells = preserveExistingCells;
                this.favorForwardCarving = favorForwardCarving;
                this.random = random;
                nextCellIncrement = new int[] { -1, 1, mazeBuilder.width, -mazeBuilder.width };
                lastMove = nextCellIncrement[random.Next(4)];
            }
            // Could move carving logic to the RandomWalk class.
            public void Update()
            {
                int nextCell = currentCell + Move();
                if (mazeBuilder.grid.ContainsEdge(currentCell, nextCell))
                {
                    // TODO: Cannot differentiate initial cells from newly carved cells. Make a mask from the old.
                    if (!preserveExistingCells)
                    {
                        mazeBuilder.CarvePassage(currentCell, nextCell);
                        mazeBuilder.numberOfCarvedPassages++;
                    }
                    currentCell = nextCell;
                    mazeBuilder.numberOfSteps++;
                }
            }

            private int Move()
            {
                if (!favorForwardCarving || (random.NextDouble() > 0.5f))
                    lastMove = nextCellIncrement[random.Next(4)];
                return lastMove;
            }
        }
        public float PercentToCarve { get; set; } = 0.6f;
        private int numberOfNewPassages;
        public int MaxWalkingDistance { get; set; } = 1000000;
        public int NumberOfWalkers { get; set; } = 4;
        public int InitialNumberOfWalkers { get; set; } = 1;
        public bool favorForwardCarving { get; set; }
        private int numberOfCarvedPassages = 0;
        private int numberOfSteps = 0;
        private float ChanceNewWalker { get; set; } = 0.5f;
        private List<Walker> walkers;
        private bool preserveExistingCells = false;
        public MazeBuilderRandomWalk(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
            : base(width, height, nodeAccessor, edgeAccessor)
        {

        }
        public override void CreateMaze(bool preserveExistingCells = false)
        {
            if (!preserveExistingCells)
            {
                BlockRegion(0, width * height - 1);
            }
            numberOfNewPassages = (int) (PercentToCarve * grid.NumberOfEdges);
            this.preserveExistingCells = preserveExistingCells;
            InitializeWalkers();
        }

        private void InitializeWalkers()
        {
            walkers = new List<Walker>(NumberOfWalkers);
            for (int i = 0; i < InitialNumberOfWalkers; i++)
            {
                Walker initialWalker = new Walker();
                // Initial start is placed randomly avoiding the borders. Assumes height > 2.
                int startCell = RandomGenerator.Next(width - 2) + 1;
                int heightCheck = (height > 2) ? RandomGenerator.Next(height - 2) + 1 : height - 1;
                startCell += width * heightCheck;

                initialWalker.StartWalker(this, startCell, preserveExistingCells, favorForwardCarving, RandomGenerator);
                walkers.Add(initialWalker);
            }

            PerformWalk();
        }

        private void PerformWalk()
        {
            while (true)
            {
                foreach(var walker in walkers)
                {
                    walker.Update();
                    if (numberOfCarvedPassages < numberOfNewPassages && numberOfSteps < MaxWalkingDistance)
                        continue;
                    return;
                }
                if((walkers.Count < NumberOfWalkers) && (this.RandomGenerator.NextDouble() < ChanceNewWalker))
                    {
                    Walker newWalker = new Walker();
                    int startCell = walkers[RandomGenerator.Next(walkers.Count)].currentCell;
                    newWalker.StartWalker(this, startCell, preserveExistingCells, favorForwardCarving, this.RandomGenerator);
                    walkers.Add(newWalker);

                }
            }
        }
    }
}