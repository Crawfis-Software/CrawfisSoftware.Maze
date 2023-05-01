using CrawfisSoftware.Collections;
using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Encoding for maze's that can prescribe the "direction" from the start (Entrance to an exit). Alternative exits from main paths.
    /// </summary>
    public enum EdgeFlow { None, Entrance, PrimaryExit, SecondaryExit, ThirdExit };

    /// <summary>
    /// Metrics on the overall Maze.
    /// </summary>
    public struct MazeMetrics
    {
        public Nullable<int> SolutionPathLength;
        public Nullable<int> NumberOfEmptyCells;
        public Nullable<int> NumberOfDeadEndCells;
        public Nullable<int> NumberOfStraightCells;
        public Nullable<int> NumberOfTurnCells;
        public Nullable<int> NumberOfTJuntionCells;
        public Nullable<int> NumberOfCrossJunctionCells;
        public Nullable<int> NumberOfSolidCells;
        public Nullable<int> NumberOfUndefinedCells;
        // Todo
        public Nullable<int> MaxBranchLevel;
        // Todo
        public Nullable<int> MaxDeadEndLength;
        public Nullable<int> MaxDistanceFromStart;
        public Nullable<int> MaxDistanceToEnd;
    }

    public struct MazeCellMetrics
    {
        public Nullable<int> DistanceFromStart;
        public Nullable<int> DistanceToEnd;
        public Nullable<EdgeFlow> LeftEdgeFlow;
        public Nullable<EdgeFlow> TopEdgeFlow;
        public Nullable<EdgeFlow> RightEdgeFlow;
        public Nullable<EdgeFlow> BottomEdgeFlow;
        // Todo
        public Nullable<int> BranchLevel;
        // Todo
        public Nullable<int> DistanceToSolution;
        public Nullable<int> Parent;
    }

    public class MazeMetricsComputations
    {
        private Maze<int, int> _maze;
        private int[] _distancesFromStart;
        private int[] _distancesToEnd;
        private int[] _distancesFromSolutionPath;
        private int[] _parents;
        private EdgeFlow[] _leftEdgeFlows;
        private EdgeFlow[] _topEdgeFlows;
        private EdgeFlow[] _rightEdgeFlows;
        private EdgeFlow[] _bottomEdgeFlows;
        private int[] __branchLevels;
        private int _width;
        private int _height;
        private bool _areDistancesFromStartComputed, _areDistancesToEndComputed, _areDirectionsFromStartComputed, _areBranchLevelsComputed, _areDistancesFromSolutionPathComputed;
        private MazeMetrics _overallMetrics;

        public MazeMetrics OverallMetrics { get { return _overallMetrics; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maze">THe maze to compute analytics on.</param>
        public MazeMetricsComputations(Maze<int, int> maze)
        {
            _maze = maze;
            _width = maze.Width;
            _height = maze.Height;
        }

        public MazeCellMetrics CellMetrics(int row, int column)
        {
            return CellMetrics(row * _width + column);
        }

        public MazeCellMetrics CellMetrics(int cellIndex)
        {
            var cellMetrics = new MazeCellMetrics();
            if (_areDirectionsFromStartComputed)
            {
                cellMetrics.DistanceFromStart = _distancesFromStart[cellIndex];
                cellMetrics.Parent = _parents[cellIndex];
            }
            if (_areDistancesToEndComputed)
                cellMetrics.DistanceToEnd = _distancesToEnd[cellIndex];
            if (_areDirectionsFromStartComputed)
            {
                cellMetrics.LeftEdgeFlow = _leftEdgeFlows[cellIndex];
                cellMetrics.TopEdgeFlow = _topEdgeFlows[cellIndex];
                cellMetrics.RightEdgeFlow = _rightEdgeFlows[cellIndex];
                cellMetrics.BottomEdgeFlow = _bottomEdgeFlows[cellIndex];
            }
            if (_areDistancesFromSolutionPathComputed)
                cellMetrics.DistanceToSolution = _distancesFromSolutionPath[cellIndex];
            if (_areBranchLevelsComputed)
                cellMetrics.BranchLevel = __branchLevels[cellIndex];
            return cellMetrics;
        }

        /// <summary>
        /// Compute all available per cell Metrics.
        /// </summary>
        /// <param name="random"></param>
        public void ComputeAllMetrics(Random random)
        {
            ComputeCellCounts();
            ComputeDistancesFromStart();
            ComputeDistancesToEnd();
            DirectionsFromStart();
            AddSecondaryExitsOnPath();
            RandomlyAssignSecondaryExits(random);
            RandomlyAssignTertiaryExits(random);
            ComputeDistanceFromSolutionPath();
            ComputeBranchLevels();
        }

        private void ComputeCellCounts()
        {
            int occupiedCells = 0;
            int crossSections = MazeQuery.CrossSections(_maze).Count();
            _overallMetrics.NumberOfCrossJunctionCells = crossSections;
            occupiedCells += crossSections;
            int tJunctions = MazeQuery.TJunctions(_maze).Count();
            _overallMetrics.NumberOfTJuntionCells = tJunctions;
            occupiedCells += tJunctions;
            int straights = MazeQuery.Straights(_maze).Count();
            _overallMetrics.NumberOfStraightCells = straights;
            occupiedCells += straights;
            int turns = MazeQuery.Turns(_maze).Count();
            _overallMetrics.NumberOfTurnCells = turns;
            occupiedCells += turns;
            int deadEnds = MazeQuery.DeadEnds(_maze).Count();
            _overallMetrics.NumberOfDeadEndCells = deadEnds;
            occupiedCells += deadEnds;
            int solids = MazeQuery.MatchingExactly(_maze, Direction.None).Count();
            occupiedCells += solids;
            _overallMetrics.NumberOfSolidCells = solids;
            _overallMetrics.NumberOfUndefinedCells = _width * _height - occupiedCells;
        }

        public void ComputeDistanceFromSolutionPath()
        {
            throw new NotImplementedException();
        }

        public void ComputeBranchLevels()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compute the distance (in number of cells) from the start.
        /// </summary>
        public void ComputeDistancesFromStart()
        {
            int[] distances = new int[_width * _height];
            int furthestDistance = 0;
            var graphEnumerator = new IndexedGraphEdgeEnumerator<int, int>(_maze, new QueueAdaptor<IIndexedEdge<int>>());
            var breadthFirstSearch = graphEnumerator.TraverseGraph(_maze.StartCell);
            foreach (var edge in breadthFirstSearch)
            {
                int node = edge.To;
                int parent = edge.From;
                int currentDistance = distances[parent] + 1;
                distances[node] = currentDistance;
                _parents[node] = parent;
                furthestDistance = (furthestDistance < currentDistance) ? currentDistance : furthestDistance;
            }
            _distancesFromStart = distances;
            _overallMetrics.MaxDistanceFromStart = furthestDistance;
            _areDistancesFromStartComputed = true;
        }

        /// <summary>
        /// Compute the distance (in number of cells) to reach the end.
        /// </summary>
        public void ComputeDistancesToEnd()
        {
            int[] distances = new int[_width * _height];
            int furthestDistance = 0;
            var graphEnumerator = new IndexedGraphEdgeEnumerator<int, int>(_maze, new QueueAdaptor<IIndexedEdge<int>>());
            var breadthFirstSearch = graphEnumerator.TraverseGraph(_maze.EndCell);
            foreach (var edge in breadthFirstSearch)
            {
                int node = edge.To;
                int parent = edge.From;
                int currentDistance = distances[parent] + 1;
                distances[node] = currentDistance;
                furthestDistance = (furthestDistance < currentDistance) ? currentDistance : furthestDistance;
            }
            _distancesToEnd = distances;
            _overallMetrics.MaxDistanceToEnd = furthestDistance;
            _areDistancesToEndComputed = true;
        }

        /// <summary>
        /// Computes for each edge of a cell whether it is reached from the start (Entrance) or not (and Exit - In this case always a PrimaryExit).
        /// </summary>
        /// <seealso cref="AddSecondaryExistsOnPath"/>
        /// <seealso cref="RandomlyAssignSecondaryExits"/>
        public void DirectionsFromStart()
        {
            int solutionPathLength = 0;
            _leftEdgeFlows = new EdgeFlow[_width * _height];
            _topEdgeFlows = new EdgeFlow[_width * _height];
            _rightEdgeFlows = new EdgeFlow[_width * _height];
            _bottomEdgeFlows = new EdgeFlow[_width * _height];
            var graphEnumerator = new IndexedGraphEdgeEnumerator<int, int>(_maze, new QueueAdaptor<IIndexedEdge<int>>());
            var breadthFirstSearch = graphEnumerator.TraverseGraph(_maze.StartCell);
            foreach (var edge in breadthFirstSearch)
            {
                int node = edge.To;
                int parent = edge.From;
                Direction flow = DirectionExtensions.GetEdgeDirection(node, parent, _width);
                Direction cellOpenings = _maze.GetDirection(node % _width, node / _width);
                _leftEdgeFlows[node] = (cellOpenings & Direction.W) == Direction.W ? ((flow == Direction.W) ? EdgeFlow.Entrance : EdgeFlow.PrimaryExit) : EdgeFlow.None;
                _topEdgeFlows[node] = (cellOpenings & Direction.N) == Direction.N ? ((flow == Direction.N) ? EdgeFlow.Entrance : EdgeFlow.PrimaryExit) : EdgeFlow.None;
                _rightEdgeFlows[node] = (cellOpenings & Direction.E) == Direction.E ? ((flow == Direction.E) ? EdgeFlow.Entrance : EdgeFlow.PrimaryExit) : EdgeFlow.None;
                _bottomEdgeFlows[node] = (cellOpenings & Direction.S) == Direction.S ? ((flow == Direction.S) ? EdgeFlow.Entrance : EdgeFlow.PrimaryExit) : EdgeFlow.None;
                solutionPathLength++;
            }
            _overallMetrics.SolutionPathLength = solutionPathLength;
            _areDirectionsFromStartComputed = true;
        }

        /// <summary>
        /// Convert multiple PrimaryExits on T-junctions and Cross-Junctions to Secondary Exits along the solution path to the maze.
        /// </summary>
        /// <remarks>Calls DirectionsFromStart is not called already.</remarks>
        public void AddSecondaryExitsOnPath()
        {
            AddSecondaryExitsOnPath(_maze.StartCell, _maze.EndCell);
        }

        /// <summary>
        /// Convert multiple PrimaryExits on T-junctions and Cross-Junctions to Secondary Exits along the path from the specified startingCell to the endingCell.
        /// </summary>
        /// <param name="startingCell">The starting cell of the path to apply directions to.</param>
        /// <param name="endingCell">The ending cell of the path to apply directions to.</param>
        /// <remarks>Calls DirectionsFromStart is not called already.</remarks>
        public void AddSecondaryExitsOnPath(int startingCell, int endingCell)
        {
            if (!_areDirectionsFromStartComputed) DirectionsFromStart();

            foreach (var edge in PathQuery<int, int>.FindPath(_maze, startingCell, endingCell))
            {
                int node = edge.To;
                int parent = edge.From;
                // Modify parent edges that are PrimaryExit, but not on the path to this cell.
                Direction flow = DirectionExtensions.GetEdgeDirection(parent, node, _width);
                _leftEdgeFlows[parent] = (_leftEdgeFlows[parent] == EdgeFlow.PrimaryExit) && ((flow & Direction.W) != Direction.None) ? EdgeFlow.SecondaryExit : _leftEdgeFlows[parent];
                _topEdgeFlows[parent] = (_topEdgeFlows[parent] == EdgeFlow.PrimaryExit) && ((flow & Direction.N) != Direction.None) ? EdgeFlow.SecondaryExit : _topEdgeFlows[parent];
                _rightEdgeFlows[parent] = (_rightEdgeFlows[parent] == EdgeFlow.PrimaryExit) && ((flow & Direction.E) != Direction.None) ? EdgeFlow.SecondaryExit : _rightEdgeFlows[parent];
                _bottomEdgeFlows[parent] = (_bottomEdgeFlows[parent] == EdgeFlow.PrimaryExit) && ((flow & Direction.S) != Direction.None) ? EdgeFlow.SecondaryExit : _bottomEdgeFlows[parent];
            }
        }

        /// <summary>
        /// Finds all Cross-sections and T-Junctions where the EdgeFlow directions contain multiple PrimaryExits and converts all but one to SecondaryExits.
        /// </summary>
        /// <param name="random">A Random number generator</param>
        public void RandomlyAssignSecondaryExits(Random random)
        {
            List<EdgeFlow[]> candidates = new List<EdgeFlow[]>(3);
            foreach (var cell in MazeQuery.CrossSections(_maze).Concat(MazeQuery.TJunctions(_maze)))
            {
                candidates.Clear();
                int cellIndex = cell.Row * _width + cell.Column;
                var directions = _maze.GetDirection(cell.Column, cell.Row);
                if (((directions & Direction.W) == Direction.W) && _leftEdgeFlows[cellIndex] == EdgeFlow.PrimaryExit)
                    candidates.Add(_leftEdgeFlows);
                if (((directions & Direction.N) == Direction.N) && _topEdgeFlows[cellIndex] == EdgeFlow.PrimaryExit)
                    candidates.Add(_topEdgeFlows);
                if (((directions & Direction.E) == Direction.E) && _rightEdgeFlows[cellIndex] == EdgeFlow.PrimaryExit)
                    candidates.Add(_rightEdgeFlows);
                if (((directions & Direction.S) == Direction.S) && _bottomEdgeFlows[cellIndex] == EdgeFlow.PrimaryExit)
                    candidates.Add(_bottomEdgeFlows);
                if (candidates.Count == 3)
                {
                    int targetIndex = random.Next(3);
                    EdgeFlow[] target = candidates[targetIndex];
                    target[cellIndex] = EdgeFlow.SecondaryExit;
                    candidates.RemoveAt(targetIndex);
                }
                if (candidates.Count == 2)
                {
                    int targetIndex = random.Next(2);
                    EdgeFlow[] target = candidates[targetIndex];
                    target[cellIndex] = EdgeFlow.SecondaryExit;
                }
            }
        }

        /// <summary>
        /// Finds all Cross-sections where the EdgeFlow directions contain multiple SecondaryExits and converts one of them to a ThirdExit.
        /// </summary>
        /// <param name="random">A Random number generator</param>
        public void RandomlyAssignTertiaryExits(Random random)
        {
            List<EdgeFlow[]> candidates = new List<EdgeFlow[]>(2);
            foreach (var cell in MazeQuery.CrossSections(_maze))
            {
                candidates.Clear();
                int cellIndex = cell.Row * _width + cell.Column;
                var directions = _maze.GetDirection(cell.Column, cell.Row);
                if (((directions & Direction.W) == Direction.W) && _leftEdgeFlows[cellIndex] == EdgeFlow.SecondaryExit)
                    candidates.Add(_leftEdgeFlows);
                if (((directions & Direction.N) == Direction.N) && _topEdgeFlows[cellIndex] == EdgeFlow.SecondaryExit)
                    candidates.Add(_topEdgeFlows);
                if (((directions & Direction.E) == Direction.E) && _rightEdgeFlows[cellIndex] == EdgeFlow.SecondaryExit)
                    candidates.Add(_rightEdgeFlows);
                if (((directions & Direction.S) == Direction.S) && _bottomEdgeFlows[cellIndex] == EdgeFlow.SecondaryExit)
                    candidates.Add(_bottomEdgeFlows);
                if (candidates.Count == 2)
                {
                    int targetIndex = random.Next(2);
                    EdgeFlow[] target = candidates[targetIndex];
                    target[cellIndex] = EdgeFlow.ThirdExit;
                }
            }
        }
    }
}