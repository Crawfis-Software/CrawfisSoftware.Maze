using CrawfisSoftware.Collections.Graph;

using System.Collections.Generic;
using System.Text;

namespace CrawfisSoftware.Maze
{
    /// <summary>
    /// A grid with some edges blocked and others open
    /// </summary>
    /// <typeparam name="N">The type used for node labels</typeparam>
    /// <typeparam name="E">The type used for edge weights</typeparam>
    public class Maze<N, E> : IIndexedGraph<N, E>, ITransposeIndexedGraph<N, E>
    {
        /// <summary>
        /// Get the grid that is the basis for the maze
        /// </summary>
        public Grid<N, E> Grid { get { return grid; } }
        /// <value>
        /// Get the width in the number of grid cells
        /// </value>
        public int Width { get { return grid.Width; } }

        /// <value>
        /// Get the height in the number of grid cells
        /// </value>
        public int Height { get { return grid.Height; } }

        /// <summary>
        /// The starting cell index for the maze. Cell indices go from bottom-left across a row to top-right.
        /// </summary>
        public int StartCell { get; }

        /// <summary>
        /// The end cell index for the maze. Cell indices go from bottom-left across a row to top-right.
        /// </summary>
        public int EndCell { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="grid">A grid to use as the basic structure of the maze</param>
        /// <param name="directions">A 2D array of Direction flags that specify the maze.</param>
        /// <param name="startCellIndex">The starting cell index for the maze. Cell indices go from 
        /// bottom-left across a row to top-right.</param>
        /// <param name="endCellIndex">The ending cell index for the maze. Cell indices go from 
        /// bottom-left across a row to top-right.</param>
        /// <remarks>Made access internal to prevent changes to the directions array.</remarks>
        internal Maze(Grid<N, E> grid, Direction[,] directions, int startCellIndex, int endCellIndex)
        {
            // Todo: Check that the grid size and the direction lengths are the same
            this.grid = grid;
            this.directions = directions;
            this.StartCell = startCellIndex;
            this.EndCell = endCellIndex;
            this.NumberOfNodes = System.Linq.Enumerable.Count<int>(this.Nodes);
            this.NumberOfEdges = System.Linq.Enumerable.Count<IIndexedEdge<E>>(this.Edges);
        }

        /// <summary>
        /// Get the set of opening directions in the current cell
        /// </summary>
        /// <param name="column">The i index of the cell</param>
        /// <param name="row">The j index of the cell</param>
        /// <returns>A set of Direction flags</returns>
        public Direction GetDirection(int column, int row)
        {
            return directions[column, row];
        }

        #region IIndexedGraph<N,E> Members
        /// <inheritdoc/>
        public int NumberOfEdges
        {
            get; private set;
        }

        /// <inheritdoc/>
        public int NumberOfNodes
        {
            get; private set;
        }

        /// <inheritdoc/>
        /// <remarks>If the node for a maze has no directions specified it is not output.</remarks>
        public IEnumerable<int> Nodes
        {
            get
            {
                foreach (int node in grid.Nodes)
                {
                    int row = node / Width;
                    int column = node % Width;
                    if (directions[column, row] != Direction.None && directions[column, row] != Direction.Undefined)
                        yield return node;
                }
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public N GetNodeLabel(int nodeIndex)
        {
            return grid.GetNodeLabel(nodeIndex);
        }

        /// <inheritdoc/>
        public IEnumerable<int> Neighbors(int nodeIndex)
        {
            foreach (int index in grid.Neighbors(nodeIndex))
            {
                if (ContainsEdge(nodeIndex, index))
                    yield return index;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IIndexedEdge<E>> OutEdges(int nodeIndex)
        {
            foreach (var outEdge in grid.OutEdges(nodeIndex))
            {
                if (ContainsEdge(outEdge.From, outEdge.To))
                {
                    yield return outEdge;
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<int> Parents(int nodeIndex)
        {
            return Neighbors(nodeIndex);
        }

        /// <inheritdoc/>
        public IEnumerable<IIndexedEdge<E>> InEdges(int nodeIndex)
        {
            foreach (var outEdge in grid.InEdges(nodeIndex))
            {
                if (ContainsEdge(outEdge.From, outEdge.To))
                {
                    yield return outEdge;
                }
            }
        }

        /// <inheritdoc/>
        public bool ContainsEdge(int fromNode, int toNode)
        {
            if (grid.ContainsEdge(fromNode, toNode))
            {
                _ = grid.TryGetGridLocation(fromNode,
                                            out int fromColumn,
                                            out int fromRow);
                _ = grid.TryGetGridLocation(toNode,
                                            out int toColumn,
                                            out int toRow);
                grid.DirectionLookUp(fromColumn, fromRow, toColumn, toRow, out Direction dir);
                if ((directions[fromColumn, fromRow] & dir) == dir)
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public E GetEdgeLabel(int fromNode, int toNode)
        {
            return grid.GetEdgeLabel(fromNode, toNode);
        }

        /// <inheritdoc/>
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
        /// <inheritdoc/>
        public IIndexedGraph<N, E> Transpose()
        {
            // Todo: should be a deep copy
            // Todo: This needs to be implemented to support direction mazes
            return this;
        }
        #endregion

        /// <summary>
        /// Converts the maze to an asci string representation
        /// </summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            int width = grid.Width;
            string cellSpace = "   ";
            const string cellFilled = "...";
            StringBuilder mazeString = new StringBuilder(width);
            mazeString.Append("+");
            for (int i = 0; i < width; i++)
            {
                Direction dirs = directions[i, grid.Height - 1];
                string northString = (dirs & Direction.N) == Direction.N ? cellSpace : "---";
                mazeString.Append(northString);
                mazeString.Append("+");
            }
            mazeString.AppendLine();
            StringBuilder rowBody = new StringBuilder(width * 4);
            StringBuilder bottomOfRow = new StringBuilder(width * 4);
            // For each row we will have two strings.
            for (int row = grid.Height - 1; row >= 0; row--)
            {
                rowBody.Remove(0, rowBody.Length);
                Direction dirs = directions[0, row];
                string westString = (dirs & Direction.W) == Direction.W ? " " : "|";
                rowBody.Append(westString);
                bottomOfRow.Remove(0, bottomOfRow.Length);
                bottomOfRow.Append("+");
                for (int column = 0; column < width; column++)
                {
                    dirs = directions[column, row];
                    string eastString = (directions[column, row] & Direction.E) == Direction.E ? " " : "|";
                    if (dirs == Direction.Undefined || dirs == Direction.None)
                        rowBody.Append(cellFilled);
                    else
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
        private readonly Grid<N, E> grid;
        private readonly Direction[,] directions;
        #endregion
    }
}
