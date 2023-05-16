using CrawfisSoftware.Collections;
using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        public PathMetric SolutionPathMetric;
        //public Nullable<int> SolutionPathLength;
        public Nullable<int> NumberOfEmptyCells;
        public Nullable<int> NumberOfDeadEndCells;
        public Nullable<int> NumberOfStraightCells;
        public Nullable<int> NumberOfTurnCells;
        public Nullable<int> NumberOfTJuntionCells;
        public Nullable<int> NumberOfCrossJunctionCells;
        public Nullable<int> NumberOfSolidCells;
        public Nullable<int> NumberOfUndefinedCells;
        public Nullable<int> MaxBranchLevel;
        public Nullable<int> MaxDeadEndLength;
        public Nullable<int> MaxDistanceFromStart;
        public Nullable<int> MaxDistanceToEnd;
        //public IList<int> SolutionPath;
    }

    public struct MazeCellMetrics
    {
        public Nullable<int> DistanceFromStart;
        public Nullable<int> DistanceToEnd;
        public Nullable<EdgeFlow> LeftEdgeFlow;
        public Nullable<EdgeFlow> TopEdgeFlow;
        public Nullable<EdgeFlow> RightEdgeFlow;
        public Nullable<EdgeFlow> BottomEdgeFlow;
        public Nullable<int> BranchLevel;
        public Nullable<int> PathDistanceToSolution;
        public Nullable<int> GridDistanceToSolution;
        public Nullable<(int solutionPathCell, Direction edge)> BranchId;
        public Nullable<int> Parent;
    }

    public class MazeMetricsComputations
    {
        private Maze<int, int> _maze;
        private int[] _distancesFromStart;
        private int[] _distancesToEnd;
        private int[] _mazeDistancesFromSolutionPath;
        private int[] _gridDistancesFromSolutionPath;
        private int[] _parents;
        private EdgeFlow[] _leftEdgeFlows;
        private EdgeFlow[] _topEdgeFlows;
        private EdgeFlow[] _rightEdgeFlows;
        private EdgeFlow[] _bottomEdgeFlows;
        private int[] _branchLevels;
        private int[] _solutionRoot;
        private  Direction[] _solutionEdge;
        private int _width;
        private int _height;
        private MazeMetrics _overallMetrics;
        private bool _areDistancesFromStartComputed, _areDistancesToEndComputed, _areDirectionsFromStartComputed, _areBranchLevelsComputed, _areMazeDistancesComputed, _areGridDistancesComputed, _isSolutionPathComputed;

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

        /// <summary>
        /// Compute all available per cell Metrics.
        /// </summary>
        /// <param name="random"></param>
        /// <param name="exitDirection">Unless the exit is a dead-end, this should be specified to indicate how the solution path exits the grid.</param>
        public void ComputeAllMetrics(Random random, Direction exitDirection = Direction.None)
        {
            ComputeCellCounts();
            ComputeDistancesFromStart();
            ComputeDistancesToEnd();
            ComputeSolutionPath();
            DirectionsFromStart();
            AddSecondaryExitsOnPath(exitDirection);
            RandomlyAssignSecondaryExits(random);
            RandomlyAssignTertiaryExits(random);
            ComputeMazeDistanceFromSolutionPath();
            ComputeGridDistanceFromSolutionPath();
            ComputeBranchLevels();
        }

        public MazeCellMetrics GetCellMetrics(int row, int column)
        {
            return GetCellMetrics(row * _width + column);
        }

        public MazeCellMetrics GetCellMetrics(int cellIndex)
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
            if (_areMazeDistancesComputed)
                cellMetrics.PathDistanceToSolution = _mazeDistancesFromSolutionPath[cellIndex];
            if (_areGridDistancesComputed)
                cellMetrics.GridDistanceToSolution = _gridDistancesFromSolutionPath[cellIndex];
            if (_areBranchLevelsComputed)
            {
                cellMetrics.BranchLevel = _branchLevels[cellIndex];
                cellMetrics.BranchId = (_solutionRoot[cellIndex], _solutionEdge[cellIndex]);
            }
            return cellMetrics;
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

        public void ComputeMazeDistanceFromSolutionPath(int unreachableDistance = 2000000)
        {
            int maxDistance = 0;
            if (!_isSolutionPathComputed) ComputeSolutionPath();
            int[] distances = new int[_width * _height];
            // Set the path distance to zero and all others to a large number.
            for (int i = 0; i < distances.Length; i++) distances[i] = unreachableDistance;
            foreach (var cellIndex in _overallMetrics.SolutionPathMetric.GridCells)
            {
                distances[cellIndex] = 0;
            }

            var mazeEnumerator = new IndexedGraphEdgeEnumerator<int, int>(_maze, new QueueAdaptor<IIndexedEdge<int>>());
            foreach (var edge in mazeEnumerator.TraverseNodes(_overallMetrics.SolutionPathMetric.GridCells))
            {
                // Set the "To" node's distance to one plus the "From" node's distance.
                int distance = distances[edge.From] + 1;
                distances[edge.To] = distance;
                maxDistance = Math.Max(maxDistance, distance);
            }
            _mazeDistancesFromSolutionPath = distances;
            _overallMetrics.MaxDeadEndLength = maxDistance;
            _areMazeDistancesComputed = true;
        }

        public void ComputeGridDistanceFromSolutionPath()
        {
            if (!_isSolutionPathComputed) ComputeSolutionPath();
            int[] distances = new int[_width * _height];
            var grid = new Graph.Grid<int, int>(_width, _height, new GetGridLabel<int>((i, j) => {return 1; }), new GetEdgeLabel<int>((i, j, v) => { return 1; }));
            var gridEnumerator = new IndexedGraphEdgeEnumerator<int, int>(grid, new QueueAdaptor<IIndexedEdge<int>>());
            foreach (var edge in gridEnumerator.TraverseNodes(_overallMetrics.SolutionPathMetric.GridCells))
            {
                // Set the "To" node's distance to one plus the "From" node's distance.
                distances[edge.To] = distances[edge.From] + 1;
            }
            _gridDistancesFromSolutionPath = distances;
            _areGridDistancesComputed = true;
        }

        public void ComputeBranchLevels()
        {
            int maxBranchLevel = 0;
            if (!_isSolutionPathComputed) ComputeSolutionPath();
            int[] branchLevels = new int[_width * _height];
            int[] solutionRoot = new int[_width * _height];
            Direction[] solutionEdge = new Direction[_width * _height];
            // Set the path distance to zero and all others to a large number.
            for (int i = 0; i < branchLevels.Length; i++) branchLevels[i] = -1;
            foreach (var cellIndex in _overallMetrics.SolutionPathMetric.GridCells)
            {
                branchLevels[cellIndex] = 0;
            }

            int currentBranchRoot = -1;
            Direction currentBranchEdge = Direction.None;
            var mazeEnumerator = new IndexedGraphEdgeEnumerator<int, int>(_maze, new QueueAdaptor<IIndexedEdge<int>>());
            foreach (var edge in mazeEnumerator.TraverseNodes(_overallMetrics.SolutionPathMetric.GridCells))
            {
                // Increment branch level if the parent's edge to me was a secondary or third exit.
                int from = edge.From;
                Direction fromEdge = DirectionExtensions.GetEdgeDirection(from, edge.To, _width);
                EdgeFlow[] edgeFlowList = null;
                if ((fromEdge & Direction.W) == Direction.W) edgeFlowList = _leftEdgeFlows;
                if ((fromEdge & Direction.N) == Direction.N) edgeFlowList = _topEdgeFlows;
                if ((fromEdge & Direction.E) == Direction.E) edgeFlowList = _rightEdgeFlows;
                if ((fromEdge & Direction.S) == Direction.S) edgeFlowList = _bottomEdgeFlows;
                EdgeFlow flowLevel = edgeFlowList[from];
                int branchLevel = branchLevels[edge.From];
                if (branchLevel == 0)
                {
                    currentBranchRoot = from;
                    currentBranchEdge = fromEdge;
                }
                else
                {
                    currentBranchRoot = solutionRoot[from];
                    currentBranchEdge = solutionEdge[from];
                }
                if(flowLevel != EdgeFlow.PrimaryExit)
                    Console.WriteLine($"Edgeflow is {flowLevel}");
                if (flowLevel == EdgeFlow.SecondaryExit || flowLevel == EdgeFlow.ThirdExit)
                {
                    branchLevel += 1;
                    maxBranchLevel = Math.Max(maxBranchLevel, branchLevel);
                }
                branchLevels[edge.To] = branchLevel;
                solutionRoot[edge.To] = currentBranchRoot;
                solutionEdge[edge.To] = currentBranchEdge;
            }
            _branchLevels = branchLevels;
            _solutionRoot = solutionRoot;
            _solutionEdge = solutionEdge;
            _overallMetrics.MaxBranchLevel = maxBranchLevel;
            _areBranchLevelsComputed = true;
        }

        /// <summary>
        /// Compute the distance (in number of cells) from the start.
        /// </summary>
        public void ComputeDistancesFromStart()
        {
            int[] distances = new int[_width * _height];
            _parents = new int[_width * _height];
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
            }
            _areDirectionsFromStartComputed = true;
        }

        public void ComputeSolutionPath()
        {
            var path = PathQuery<int, int>.FindPath(_maze, _maze.StartCell, _maze.EndCell);
            var solutionPath = new List<int>();
            solutionPath.Add(_maze.StartCell);
            foreach (var edge in path)
                solutionPath.Add(edge.To);
            _overallMetrics.SolutionPathMetric = new PathMetric(solutionPath, _maze.Width);
            _isSolutionPathComputed = true;
        }

        /// <summary>
        /// Convert multiple PrimaryExits on T-junctions and Cross-Junctions to Secondary Exits along the solution path to the maze.
        /// </summary>
        /// <remarks>Calls DirectionsFromStart is not called already.</remarks>
        public void AddSecondaryExitsOnPath(Direction exitDirection)
        {
            AddSecondaryExitsOnPath(_maze.StartCell, _maze.EndCell, exitDirection);
        }

        /// <summary>
        /// Convert multiple PrimaryExits on T-junctions and Cross-Junctions to Secondary Exits along the path from the specified startingCell to the endingCell.
        /// </summary>
        /// <param name="startingCell">The starting cell of the path to apply directions to.</param>
        /// <param name="endingCell">The ending cell of the path to apply directions to.</param>
        /// <remarks>Calls DirectionsFromStart is not called already.</remarks>
        public void AddSecondaryExitsOnPath(int startingCell, int endingCell, Direction exitDirection)
        {
            if (!_areDirectionsFromStartComputed) DirectionsFromStart();
            //if(!_isSolutionPathComputed) ComputeSolutionPath();

            int node = -1;
            int parent = -1;
            foreach (var edge in PathQuery<int, int>.FindPath(_maze, startingCell, endingCell))
            {
                node = edge.To;
                parent = edge.From;
                // Modify parent edges that are PrimaryExit, but not on the path to this cell.
                Direction flow = DirectionExtensions.GetEdgeDirection(parent, node, _width);
                _leftEdgeFlows[parent] = (_leftEdgeFlows[parent] == EdgeFlow.PrimaryExit) && ((flow & Direction.W) != Direction.W) ? EdgeFlow.SecondaryExit : _leftEdgeFlows[parent];
                _topEdgeFlows[parent] = (_topEdgeFlows[parent] == EdgeFlow.PrimaryExit) && ((flow & Direction.N) != Direction.N) ? EdgeFlow.SecondaryExit : _topEdgeFlows[parent];
                _rightEdgeFlows[parent] = (_rightEdgeFlows[parent] == EdgeFlow.PrimaryExit) && ((flow & Direction.E) != Direction.E) ? EdgeFlow.SecondaryExit : _rightEdgeFlows[parent];
                _bottomEdgeFlows[parent] = (_bottomEdgeFlows[parent] == EdgeFlow.PrimaryExit) && ((flow & Direction.S) != Direction.S) ? EdgeFlow.SecondaryExit : _bottomEdgeFlows[parent];
            }
            if(node >=0)
            {
                Direction flow = exitDirection;
                _leftEdgeFlows[node] = (_leftEdgeFlows[node] == EdgeFlow.PrimaryExit) && ((flow & Direction.W) != Direction.W) ? EdgeFlow.SecondaryExit : _leftEdgeFlows[node];
                _topEdgeFlows[node] = (_topEdgeFlows[node] == EdgeFlow.PrimaryExit) && ((flow & Direction.N) != Direction.N) ? EdgeFlow.SecondaryExit : _topEdgeFlows[node];
                _rightEdgeFlows[node] = (_rightEdgeFlows[node] == EdgeFlow.PrimaryExit) && ((flow & Direction.E) != Direction.E) ? EdgeFlow.SecondaryExit : _rightEdgeFlows[node];
                _bottomEdgeFlows[node] = (_bottomEdgeFlows[node] == EdgeFlow.PrimaryExit) && ((flow & Direction.S) != Direction.S) ? EdgeFlow.SecondaryExit : _bottomEdgeFlows[node];

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