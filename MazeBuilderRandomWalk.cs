using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawfisSoftware.Collections.Graph;
using CrawfisSoftware.Collections.Maze;

namespace CrawfisSoftware.Collections.Maze
{
    public class MazeBuilderRandomWalk : MazeBuilderAbstract<int, int>
    {
        // Having Walker in a seperate class allows for multiple walkers. Need to make this thread safe though.
        private class Walker
        {
            private MazeBuilderRandomWalk mazeBuilder;
            private int currentCell;
            private System.Random random;
            bool preserveExistingCells;
            private bool favorForwardCarving;
            private int[] nextCellIncrement;
            public void StartWalker(MazeBuilderRandomWalk mazeBuilder, int cell, bool preserveExistingCells, bool favorForwardCarving, System.Random random)
            {
                this.mazeBuilder = mazeBuilder;
                currentCell = cell;
                this.preserveExistingCells = preserveExistingCells;
                this.random = random;
                nextCellIncrement = new int[] { -1, 1, mazeBuilder.width, -mazeBuilder.width };
            }
            // Could move carving logic to the DrunkenWalk class.
            public void Update()
            {
                int nextCell = currentCell + Move();
                if (mazeBuilder.grid.ContainsEdge(currentCell, nextCell))
                {
                    if (!preserveExistingCells)
                    {
                        mazeBuilder.CarvePassage(currentCell, nextCell);
                    }
                    currentCell = nextCell;
                    mazeBuilder.numberOfCarvedPassages++;
                }
                mazeBuilder.numberOfSteps++;
            }

            private int Move()
            {
                if (favorForwardCarving)
                {
                    // TODO: Loop through random until ?? Need to save previous cell increment index;
                }
                return nextCellIncrement[random.Next(4)];
            }
        }
        public int NumberOfNewPassages { get; set; } = 1000000;
        public int MaxWalkingDistance { get; set; } = 1000000;
        public int NumberOfWalkers { get; set; } = 1;
        public bool favorForwardCarving;
        private int numberOfCarvedPassages = 0;
        private int numberOfSteps = 0;
        private List<Walker> walkers;
        public MazeBuilderRandomWalk(int width, int height, GetGridLabel<int> nodeAccessor, GetEdgeLabel<int> edgeAccessor)
            : base(width, height, DummyNodeValues, DummyEdgeValues)
        {

        }
        public MazeBuilderRandomWalk(int width, int height)
                : base(width, height, DummyNodeValues, DummyEdgeValues)
        {

        }
        public override void CreateMaze(bool preserveExistingCells = false)
        {
            InitializeWalker(preserveExistingCells);
        }

        private void InitializeWalker(bool preserveExistingCells)
        {
            if (!preserveExistingCells)
            {
                BlockRegion(0, width * height - 1);
            }
            walkers = new List<Walker>(NumberOfWalkers);
            Walker initialWalker = new Walker();
            // Initial start is placed randomly avoiding the borders. Assumes height > 2.
            System.Random random = new System.Random();
            int startCell = random.Next(width - 2) + 1;
            int heightCheck = (height > 2) ? random.Next(height - 2) + 1 : height - 1;
            startCell += width * heightCheck;

            initialWalker.StartWalker(this, startCell, preserveExistingCells, favorForwardCarving, random);
            walkers.Add(initialWalker);

            PerformWalk();
        }

        private void PerformWalk()
        {
            while(true)
            {
                foreach(var walker in walkers)
                {
                    walker.Update();
                    if (numberOfCarvedPassages < NumberOfNewPassages && numberOfSteps < MaxWalkingDistance)
                        continue;
                    return;
                }
            }
        }

        internal static int DummyNodeValues(int i, int j)
        {
            return 1;
        }
        internal static int DummyEdgeValues(int i, int j, Direction dir)
        {
            return 1;
        }
    }
}
