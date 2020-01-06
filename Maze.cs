using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrawfisSoftware.Collections.Maze
{
    public class Maze<N, E> : IIndexedGraph<N, E>, ITransposeIndexedGraph<N, E>
    {
        internal Maze(Grid<N, E> grid, Direction[,] directions)
        {
            this.grid = grid;
            this.directions = directions;
            directions[0, 0] |= Direction.S;
            directions[grid.Height - 1, grid.Width - 1] |= Direction.E;
        }
        internal Direction GetDirection(int row, int column)
        {
            return directions[row, column];
        }

        #region IIndexedGraph<N,E> Members
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

        public IEnumerable<IIndexedEdge<E>> Edges
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

        public IEnumerable<IIndexedEdge<E>> OutEdges(int nodeIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> Parents(int nodeIndex)
        {
            return Neighbors(nodeIndex);
        }

        public IEnumerable<IIndexedEdge<E>> InEdges(int nodeIndex)
        {
            throw new NotImplementedException();
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
                if ((directions[fromRow, fromColumn] & dir) == dir)
                    return true;
            }
            return false;
        }

        public E GetEdgeLabel(int fromNode, int toNode)
        {
            return grid.GetEdgeLabel(fromNode, toNode);
        }

        public bool TryGetEdgeLabel(int fromNode, int toNode, out E edge)
        {
            if (ContainsEdge(fromNode, toNode))
            {
                edge = GetEdgeLabel(fromNode, toNode);
                return true;
            }
            edge = default(E);
            return false;
        }
        #endregion

        #region ITransposeIndexedGraph<N,E> Members
        public IIndexedGraph<N, E> Transpose()
        {
            throw new NotImplementedException();
        }
        #endregion

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
                    string eastString = (directions[row, column] & Direction.E) == Direction.E ? " " : "|";
                    rowBody.Append(cellSpace);
                    rowBody.Append(eastString);
                    string southString = (directions[row, column] & Direction.S) == Direction.S ? cellSpace : "---";
                    bottomOfRow.Append(southString);
                    bottomOfRow.Append("+");
                }
                mazeString.AppendLine(rowBody.ToString());
                mazeString.AppendLine(bottomOfRow.ToString());
            }
            return mazeString.ToString();
        }

        #region Member variables
        private readonly Grid<N, E> grid;
        Dictionary<Direction, char> graphCodes = new Dictionary<Direction, char>(16);
        private readonly Direction[,] directions;
        #endregion
    }
}
