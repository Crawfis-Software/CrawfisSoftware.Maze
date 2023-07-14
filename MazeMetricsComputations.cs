using CrawfisSoftware.Collections.Graph;
using CrawfisSoftware.Collections.Path;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrawfisSoftware.Collections.Maze
{
    /// <summary>
    /// Computes metrics on a maze and allows for easy access to both global metrics and per cell metrics.
    /// </summary>
    public class MazeMetricsComputations<N, E>
    {
        private Maze<N, E> _maze;
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
        private Direction[] _solutionEdge;
        private int _width;
        private int _height;
        private MazeMetrics _overallMetrics;
        private bool _areDistancesFromStartComputed, _areDistancesToEndComputed, _areDirectionsFromStartComputed, _areBranchLevelsComputed, _areMazeDistancesComputed, _areGridDistancesComputed, _isSolutionPathComputed;

        /// <summary>
        /// Get the metrics pertaining to the entire maze.
        /// </summary>
        public MazeMetrics OverallMetrics { get { return _overallMetrics; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maze">The maze to compute analytics on.</param>
        /// <remarks>Call the various Computation methods prior to accessing data. This allows the use to pick and choose which computations should be 
        /// performed <seealso cref="ComputeAllMetrics(Random, Direction)"/>.</remarks>
        public MazeMetricsComputations(Maze<N, E> maze)
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

        /// <summary>
        /// Get metrics pertaining to a single cell.
        /// </summary>
        /// <param name="column">The grid column of the cell.</param>
        /// <param name="row">The grid row of the cell.</param>
        /// <returns>A MazeCellMetrics.</returns>
        public MazeCellMetrics GetCellMetrics(int column, int row)
        {
            return GetCellMetrics(row * _width + column);
        }

        /// <summary>
        /// Get metrics pertaining to a single cell.
        /// </summary>
        /// <param name="cellIndex">The grid cell index.</param>
        /// <returns>A MazeCellMetrics.</returns>
        public MazeCellMetrics GetCellMetrics(int cellIndex)
        {
            var cellMetrics = new MazeCellMetrics();
            if (_areDistancesFromStartComputed)
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

        /// <summary>
        /// Compute the maze-distance (path distance) from the solution path.
        /// </summary>
        /// <param name="unreachableDistance">If the cell is not reachable from the solution path, set the distance to this value.</param>
        public void ComputeMazeDistanceFromSolutionPath(int unreachableDistance = 2000000)
        {
            int maxDistance = 0;
            if (!_isSolutionPathComputed) ComputeSolutionPath();
            int[] distances = new int[_width * _height];
            // Set the path distance to zero and all others to a large number.
            for (int i = 0; i < distances.Length; i++) distances[i] = unreachableDistance;
            foreach (var cellIndex in _overallMetrics.SolutionPathMetric.Path)
            {
                distances[cellIndex] = 0;
            }

            var mazeEnumerator = new IndexedGraphEdgeEnumerator<N,E>(_maze, new QueueAdaptor<IIndexedEdge<E>>());
            foreach (var edge in mazeEnumerator.TraverseNodes(_overallMetrics.SolutionPathMetric.Path))
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

        /// <summary>
        /// Compute the non-maze, grid-based distance each cell is from the solution path.
        /// </summary>
        public void ComputeGridDistanceFromSolutionPath()
        {
            if (!_isSolutionPathComputed) ComputeSolutionPath();
            int[] distances = new int[_width * _height];
            var grid = new Graph.Grid<int, int>(_width, _height, new GetGridLabel<int>((i, j) => { return 1; }), new GetEdgeLabel<int>((i, j, v) => { return 1; }));
            var gridEnumerator = new IndexedGraphEdgeEnumerator<int, int>(grid, new QueueAdaptor<IIndexedEdge<int>>());
            foreach (var edge in gridEnumerator.TraverseNodes(_overallMetrics.SolutionPathMetric.Path))
            {
                // Set the "To" node's distance to one plus the "From" node's distance.
                // Use the Edge value (set above to always be 1) for some flexibility.
                distances[edge.To] = distances[edge.From] + edge.Value;
            }
            _gridDistancesFromSolutionPath = distances;
            _areGridDistancesComputed = true;
        }

        /// <summary>
        /// Compute branch levels for the maze, where the level is based on the number of second and third exit crossings.
        /// </summary>
        /// <param name="setBranchRootToSolution">If True (default), then the BranchID's are based on the solution path. 
        /// If false, the BranchID's are the closest ancestor with a junction (a branch).</param>
        public void ComputeBranchLevels(bool setBranchRootToSolution = true)
        {
            int maxBranchLevel = 0;
            if (!_isSolutionPathComputed) ComputeSolutionPath();
            int[] branchLevels = new int[_width * _height];
            int[] solutionRoot = new int[_width * _height];
            Direction[] solutionEdge = new Direction[_width * _height];
            // Set the path distance to zero and all others to a large number.
            for (int i = 0; i < branchLevels.Length; i++) branchLevels[i] = -1;
            foreach (var cellIndex in _overallMetrics.SolutionPathMetric.Path)
            {
                branchLevels[cellIndex] = 0;
            }

            int currentBranchRoot = -1;
            Direction currentBranchEdge = Direction.None;
            var mazeEnumerator = new IndexedGraphEdgeEnumerator<N, E>(_maze, new QueueAdaptor<IIndexedEdge<E>>());
            foreach (var edge in mazeEnumerator.TraverseNodes(_overallMetrics.SolutionPathMetric.Path))
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
                if (setBranchRootToSolution && branchLevel == 0)
                {
                    currentBranchRoot = from;
                    currentBranchEdge = fromEdge;
                }
                else
                {
                    currentBranchRoot = solutionRoot[from];
                    currentBranchEdge = solutionEdge[from];
                }
                if (flowLevel != EdgeFlow.PrimaryExit)
                    Console.WriteLine($"Edgeflow is {flowLevel}");
                if (flowLevel == EdgeFlow.SecondaryExit || flowLevel == EdgeFlow.ThirdExit)
                {
                    branchLevel += 1;
                    maxBranchLevel = Math.Max(maxBranchLevel, branchLevel);
                    if(!setBranchRootToSolution)
                    {
                        currentBranchRoot = from;
                        currentBranchEdge = fromEdge;
                    }
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
            var graphEnumerator = new IndexedGraphEdgeEnumerator<N, E>(_maze, new QueueAdaptor<IIndexedEdge<E>>());
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
            var graphEnumerator = new IndexedGraphEdgeEnumerator<N, E>(_maze, new QueueAdaptor<IIndexedEdge<E>>());
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
            var graphEnumerator = new IndexedGraphEdgeEnumerator<N, E>(_maze, new QueueAdaptor<IIndexedEdge<E>>());
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

        /// <summary>
        /// Compute metrics on the solution path for the maze.
        /// </summary>
        public void ComputeSolutionPath()
        {
            var path = PathQuery<N, E>.FindPath(_maze, _maze.StartCell, _maze.EndCell);
            var solutionPath = new List<int>();
            solutionPath.Add(_maze.StartCell);
            foreach (var edge in path)
                solutionPath.Add(edge.To);
            _overallMetrics.SolutionPathMetric = new GridPathMetrics<int, int>(new GridPath<int, int>(new Grid<int, int>(_maze.Width, _maze.Height, null, null), solutionPath));
            _isSolutionPathComputed = true;
        }

        /// <summary>
        /// Convert multiple PrimaryExits on E-junctions and Cross-Junctions to Secondary Exits along the solution path to the maze.
        /// </summary>
        /// <param name="exitDirection">Unless the exit is a dead-end, this should be specified to indicate how the solution path exits the grid.</param>
        /// <remarks>Calls DirectionsFromStart is not called already.</remarks>
        public void AddSecondaryExitsOnPath(Direction exitDirection)
        {
            AddSecondaryExitsOnPath(_maze.StartCell, _maze.EndCell, exitDirection);
        }

        /// <summary>
        /// Convert multiple PrimaryExits on E-junctions and Cross-Junctions to Secondary Exits along the path from the specified startingCell to the endingCell.
        /// </summary>
        /// <param name="startingCell">The starting cell of the path to apply directions to.</param>
        /// <param name="endingCell">The ending cell of the path to apply directions to.</param>
        /// <param name="exitDirection">Unless the exit is a dead-end, this should be specified to indicate how the solution path exits the grid.</param>
        /// <remarks>Calls DirectionsFromStart is not called already.</remarks>
        public void AddSecondaryExitsOnPath(int startingCell, int endingCell, Direction exitDirection)
        {
            if (!_areDirectionsFromStartComputed) DirectionsFromStart();
            //if(!_isSolutionPathComputed) ComputeSolutionPath();

            int node = -1;
            int parent = -1;
            foreach (var edge in PathQuery<N, E>.FindPath(_maze, startingCell, endingCell))
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
            if (node >= 0)
            {
                Direction flow = exitDirection;
                _leftEdgeFlows[node] = (_leftEdgeFlows[node] == EdgeFlow.PrimaryExit) && ((flow & Direction.W) != Direction.W) ? EdgeFlow.SecondaryExit : _leftEdgeFlows[node];
                _topEdgeFlows[node] = (_topEdgeFlows[node] == EdgeFlow.PrimaryExit) && ((flow & Direction.N) != Direction.N) ? EdgeFlow.SecondaryExit : _topEdgeFlows[node];
                _rightEdgeFlows[node] = (_rightEdgeFlows[node] == EdgeFlow.PrimaryExit) && ((flow & Direction.E) != Direction.E) ? EdgeFlow.SecondaryExit : _rightEdgeFlows[node];
                _bottomEdgeFlows[node] = (_bottomEdgeFlows[node] == EdgeFlow.PrimaryExit) && ((flow & Direction.S) != Direction.S) ? EdgeFlow.SecondaryExit : _bottomEdgeFlows[node];

            }
        }

        /// <summary>
        /// Finds all Cross-sections and E-Junctions and relabel all PrimaryExits to SecondaryExits.
        /// </summary>
        /// <param name="ignoreSolutionPath">If true, no edges on the solution path will be changed.</param>
        /// <remarks>Requires solution path metrics to be computed if ignoreSolutionPath is true.</remarks>
        public void AssignSecondaryExitsToAllJunctions(bool ignoreSolutionPath)
        {
            foreach (var cell in MazeQuery.CrossSections(_maze).Concat(MazeQuery.TJunctions(_maze)))
            {
                int cellIndex = cell.Row * _width + cell.Column;
                if (ignoreSolutionPath && _isSolutionPathComputed)
                    if(_overallMetrics.SolutionPathMetric.Path.Contains(cellIndex)) continue;

                var directions = _maze.GetDirection(cell.Column, cell.Row);
                if (((directions & Direction.W) == Direction.W) && _leftEdgeFlows[cellIndex] == EdgeFlow.PrimaryExit)
                    _leftEdgeFlows[cellIndex] = EdgeFlow.SecondaryExit;
                if (((directions & Direction.N) == Direction.N) && _topEdgeFlows[cellIndex] == EdgeFlow.PrimaryExit)
                    _topEdgeFlows[cellIndex] = EdgeFlow.SecondaryExit;
                if (((directions & Direction.E) == Direction.E) && _rightEdgeFlows[cellIndex] == EdgeFlow.PrimaryExit)
                    _rightEdgeFlows[cellIndex] = EdgeFlow.SecondaryExit;
                if (((directions & Direction.S) == Direction.S) && _bottomEdgeFlows[cellIndex] == EdgeFlow.PrimaryExit)
                    _bottomEdgeFlows[cellIndex] = EdgeFlow.SecondaryExit;
            }
        }

        /// <summary>
        /// Finds all Cross-sections and E-Junctions where the EdgeFlow directions contain multiple PrimaryExits and converts all but one to SecondaryExits.
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
    }
}