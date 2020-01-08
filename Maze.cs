using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrawfisSoftware.Collections.Maze
{
    public class Maze<N> : IIndexedGraph<N, int>, ITransposeIndexedGraph<N, int>
    {
        public int Width { get { return grid.Width; } }
        public int Height {  get { return grid.Height; } }

        internal Maze(Grid<N, int> grid, Direction[,] directions)
        {
            this.grid = grid;
            this.directions = directions;
            directions[0, 0] |= Direction.S;
            directions[grid.Width - 1, grid.Height - 1] |= Direction.E;
        }
        public Direction GetDirection(int column, int row)
        {
            return directions[column, row];
        }

        #region IIndexedGraph<N,int> Members
        public int NumberOfEdges
        {
            // A perfect maze is a tree which has N-1 edges.
            get { return grid.NumberOfNodes - 1; }
        }

        public int NumberOfNodes
        {
            get { return grid.NumberOfNodes; }
        }

        public IEnumerable<int> Nodes
        {
            get { return grid.Nodes; }
        }

        public IEnumerable<IIndexedEdge<int>> Edges
        {
            get
            {
                foreach (var edge in grid.Edges)
                {
                    if (ContainsEdge(edge.From, edge.To))
                        yield return edge;
                }
            }
        }

        public N GetNodeLabel(int nodeIndex)
        {
            return grid.GetNodeLabel(nodeIndex);
        }

        public IEnumerable<int> Neighbors(int nodeIndex)
        {
            foreach (int index in grid.Neighbors(nodeIndex))
            {
                if (ContainsEdge(nodeIndex, index))
                    yield return index;
            }
        }

        public IEnumerable<IIndexedEdge<int>> OutEdges(int fromNode)
        {
            int toNode = fromNode + 1;
            if (grid.ContainsEdge(fromNode, toNode))
            {
                int fromRow, fromColumn, toRow, toColumn;
                grid.TryGetGridLocation(fromNode, out fromColumn, out fromRow);
                grid.TryGetGridLocation(toNode, out toColumn, out toRow);
                Direction dir;
                grid.DirectionLookUp(fromColumn, fromRow, toColumn, toRow, out dir);
                if ((directions[fromColumn, fromRow] & dir) == dir)
                {
                    int label = grid.GetEdgeLabel(fromNode, toNode, dir);
                    yield return new IndexedEdge<int>(fromNode, toNode, label);
                }
            }
            toNode = fromNode - 1;
            if (grid.ContainsEdge(fromNode, toNode))
            {
                int fromRow, fromColumn, toRow, toColumn;
                grid.TryGetGridLocation(fromNode, out fromColumn, out fromRow);
                grid.TryGetGridLocation(toNode, out toColumn, out toRow);
                Direction dir;
                grid.DirectionLookUp(fromColumn, fromRow, toColumn, toRow, out dir);
                if ((directions[fromColumn, fromRow] & dir) == dir)
                {
                    int label = grid.GetEdgeLabel(fromNode, toNode, dir);
                    yield return new IndexedEdge<int>(fromNode, toNode, label);
                }
            }
            toNode = fromNode + Width;
            if (grid.ContainsEdge(fromNode, toNode))
            {
                int fromRow, fromColumn, toRow, toColumn;
                grid.TryGetGridLocation(fromNode, out fromColumn, out fromRow);
                grid.TryGetGridLocation(toNode, out toColumn, out toRow);
                Direction dir;
                grid.DirectionLookUp(fromColumn, fromRow, toColumn, toRow, out dir);
                if ((directions[fromColumn, fromRow] & dir) == dir)
                {
                    int label = grid.GetEdgeLabel(fromNode, toNode, dir);
                    yield return new IndexedEdge<int>(fromNode, toNode, label);
                }
            }
            toNode = fromNode - Width;
            if (grid.ContainsEdge(fromNode, toNode))
            {
                int fromRow, fromColumn, toRow, toColumn;
                grid.TryGetGridLocation(fromNode, out fromColumn, out fromRow);
                grid.TryGetGridLocation(toNode, out toColumn, out toRow);
                Direction dir;
                grid.DirectionLookUp(fromColumn, fromRow, toColumn, toRow, out dir);
                if ((directions[fromColumn, fromRow] & dir) == dir)
                {
                    int label = grid.GetEdgeLabel(fromNode, toNode, dir);
                    yield return new IndexedEdge<int>(fromNode, toNode, label);
                }
            }
        }

        public IEnumerable<int> Parents(int nodeIndex)
        {
            return Neighbors(nodeIndex);
        }

        public IEnumerable<IIndexedEdge<int>> InEdges(int nodeIndex)
        {
            return OutEdges(nodeIndex);
            //foreach (var edge in grid.InEdges(nodeIndex))
            //{
            //    yield return edge;
            //}
        }

        public bool ContainsEdge(int fromNode, int toNode)
        {
            if (grid.ContainsEdge(fromNode, toNode))
            {
                int fromRow, fromColumn, toRow, toColumn;
                grid.TryGetGridLocation(fromNode, out fromColumn, out fromRow);
                grid.TryGetGridLocation(toNode, out toColumn, out toRow);
                Direction dir;
                grid.DirectionLookUp(fromColumn, fromRow, toColumn, toRow, out dir);
                if ((directions[fromColumn, fromRow] & dir) == dir)
                    return true;
            }
            return false;
        }

        public int GetEdgeLabel(int fromNode, int toNode)
        {
            return grid.GetEdgeLabel(fromNode, toNode);
        }

        public bool TryGetEdgeLabel(int fromNode, int toNode, out int edge)
        {
            if (ContainsEdge(fromNode, toNode))
            {
                edge = GetEdgeLabel(fromNode, toNode);
                return true;
            }
            edge = default(int);
            return false;
        }
        #endregion

        #region ITransposeIndexedGraph<N,int> Members
        public IIndexedGraph<N, int> Transpose()
        {
            // Todo: This should probably be a deep copy
            return this;
        }
        #endregion

        int UpdatedEdgeFunction(int i, int j, Direction dir)
        {
            int edgeValue = grid.GetEdgeLabel(i, j, dir);
            if (!ContainsEdge(i, j))
                edgeValue += 1000000000;
            return edgeValue;
        }

        public override string ToString()
        {
            int width = grid.Width;
            StringBuilder mazeString = new StringBuilder(width);
            mazeString.Append("+");
            for (int i = 0; i < width; i++)
                mazeString.Append("---+");
            mazeString.AppendLine();
            StringBuilder rowBody = new StringBuilder(width * 4);
            StringBuilder bottomOfRow = new StringBuilder(width * 4);
            string cellSpace = "   ";
            // For each row we will have two strings.
            for (int row = grid.Height - 1; row >= 0; row--)
            {
                rowBody.Remove(0, rowBody.Length);
                rowBody.Append("|");
                bottomOfRow.Remove(0, bottomOfRow.Length);
                bottomOfRow.Append("+");
                for (int column = 0; column < width; column++)
                {
                    string eastString = (directions[column, row] & Direction.E) == Direction.E ? " " : "|";
                    rowBody.Append(cellSpace);
                    rowBody.Append(eastString);
                    string southString = (directions[column, row] & Direction.S) == Direction.S ? cellSpace : "---";
                    bottomOfRow.Append(southString);
                    bottomOfRow.Append("+");
                }
                mazeString.AppendLine(rowBody.ToString());
                mazeString.AppendLine(bottomOfRow.ToString());
            }
            return mazeString.ToString();
        }

        #region Member variables
        private readonly Grid<N, int> grid;
        Dictionary<Direction, char> graphCodes = new Dictionary<Direction, char>(16);
        private readonly Direction[,] directions;
        #endregion
    }
}
