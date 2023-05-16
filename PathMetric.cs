using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

// Todo: Move to Grid namespace of some other namespace.
namespace CrawfisSoftware.Collections.Maze
{

    /// <summary>
    /// Data structure to hold path metrics on a grid.
    /// </summary>
    public class PathMetric
    {
        /// <summary>
        /// A list of the grid cell indices that the path passes through.
        /// </summary>
        public List<int> gridCells;
        /// <summary>
        /// The underlying grid width in terms of the number of columns.
        /// </summary>
        public int gridWidth;
        /// <summary>
        /// A (Column, Row) value tuple of the starting cell.
        /// </summary>
        public (int Column, int Row) StartingCell;
        /// <summary>
        /// A (Column, Row) value tuple of the ending cell.
        /// </summary>
        public (int Column, int Row) EndingCell;
        /// <summary>
        /// The length of the path in the number of grid cells.
        /// </summary>
        public int PathLength;
        /// <summary>
        /// The maximum number of consecutive horizontal or vertical straights.
        /// </summary>
        public int MaximumConsecutiveTurns;
        /// <summary>
        /// The maximum number of consecutive turns (left or right).
        /// </summary>
        public int MaximumConsecutiveStraights;
        /// <summary>
        /// A string representing the path movements where S implies go straight, L implies go left, and R implies go right. This can be easily searched for patterns.
        /// </summary>
        /// <remarks>The string path is 2 characters shorter than the path length due to the start and end cells considered as dead-ends.</remarks>
        /// <seealso cref="System.Text.RegularExpressions"/>
        public string TurtlePath;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gridCells">The path defined as a sequence of grid cells on a grid with the given width.</param>
        /// <param name="gridWidth">The width of the grid.</param>
        public PathMetric(List<int> gridCells, int gridWidth)
        {
            this.gridCells = gridCells;
            this.gridWidth = gridWidth;
            StartingCell = (gridCells[0] % gridWidth, gridCells[0] / gridWidth);
            EndingCell = (gridCells[gridCells.Count - 1] % gridWidth, gridCells[gridCells.Count - 1] / gridWidth);
            PathLength = gridCells.Count;
            MaximumConsecutiveTurns = 0;
            MaximumConsecutiveStraights = 0;
            StringBuilder path = new StringBuilder(gridCells.Count);
            int numberOfTurns = 0;
            int numberOfStraights = 0;
            for(int i = 1; i < gridCells.Count-1; i++)
            {
                Direction cellDirection = DirectionExtensions.GetEdgeDirection(gridCells[i-1], gridCells[i], gridWidth);
                cellDirection |= DirectionExtensions.GetEdgeDirection(gridCells[i+1], gridCells[i], gridWidth);
                if (cellDirection.IsStraight())
                {
                    path.Append("S");
                    numberOfTurns = 0;
                    numberOfStraights++;
                    MaximumConsecutiveStraights = (MaximumConsecutiveStraights >= numberOfStraights) ? MaximumConsecutiveStraights : numberOfStraights;
                }
                if (cellDirection.IsTurn())
                {
                    // Building a little logic table of (i-1)->i versus i->i+1 yeilds this.
                    int deltai = gridCells[i] - gridCells[i - 1];
                    int deltaii = gridCells[i+1] - gridCells[i];
                    int testValue = (Math.Abs(deltai)-2) * deltai * deltaii;
                    if(testValue < 0)
                        path.Append("L");
                    else
                        path.Append("R");
                    numberOfStraights = 0;
                    numberOfTurns++;
                    MaximumConsecutiveTurns = (MaximumConsecutiveTurns >= numberOfTurns) ? MaximumConsecutiveTurns : numberOfTurns;
                }
            }
            TurtlePath = path.ToString();
        }

        /// <summary>
        /// Searches the TurtlePath string for the regular expression and returns the starting cell for each instance it encounters.
        /// </summary>
        /// <param name="regex">A Regular Expression in the System.Text.RegularExpression.Regex format.</param>
        /// <returns>The starting index for the pattern for each occurance.</returns>
        /// <remarks>Note that the pattern usually starts at the cell before. For instance a left turn that starts at i-1, goes through i to i+width, will return i-1, not i.</remarks>
        public IEnumerable<int> Search(Regex regex)
        {
            var matches = regex.Matches(TurtlePath);
            foreach (Match match in matches)
            {
                int stringIndex = match.Index;
                int cellIndex = gridCells[stringIndex];
                yield return cellIndex;
            }
        }

        /// <summary>
        /// Enumerates all of the U-turns in TurtlePath string (aka, all "RR" and "LL" substrings).
        /// </summary>
        /// <returns>The starting index for the pattern for each occurance.</returns>
        /// <remarks>Note that the pattern usually starts at the cell before. For instance a left turn that starts at i-1, goes through i to i+width, will return i-1, not i.</remarks>
        /// <seealso cref="Search(Regex)"/>
        public IEnumerable<int> UTurns()
        {
            Regex regex = new Regex("(RR|LL)", RegexOptions.Compiled);
            return Search(regex);
        }

        /// <summary>
        /// Enumerates all of the consecutive straights ("S") in TurtlePath string with a length greater than or equal to specified length.
        /// </summary>
        /// <returns>The starting index for the pattern for each occurance.</returns>
        /// <remarks>Note that the pattern usually starts at the cell before. For instance a left turn that starts at i-1, goes through i to i+width, will return i-1, not i.</remarks>
        /// <seealso cref="Search(Regex)"/>
        public IEnumerable<int> StraightAways(int straightLength)
        {
            //string pattern = "S{" + straightLength + "}";
            //Regex regex = new Regex(pattern, RegexOptions.Compiled);
            //return Search(regex);
            StringBuilder minStraightsSequence = new StringBuilder();
            for (int i = 0; i < straightLength; i++)
                minStraightsSequence.Append("S");
            string subString = TurtlePath;
            int stringIndex = subString.IndexOf(minStraightsSequence.ToString());
            int turtleIndex = 0;
            while (stringIndex >=0)
            {
                int cellIndex = gridCells[stringIndex+turtleIndex];
                yield return cellIndex;
                while (stringIndex < subString.Length && subString[stringIndex] == 'S') stringIndex++;
                subString = subString.Substring(stringIndex);
                turtleIndex += stringIndex;
                stringIndex = subString.IndexOf(minStraightsSequence.ToString());
            }
        }

        /// <summary>
        /// Calculate the ratio of straights to turns within a specified window centered on the specified path index.
        /// </summary>
        /// <param name="pathIndex">The index into the List of grid cells that define the path, not a grid index itself.</param>
        /// <param name="halfWindowSize">The half window size to use in the analysis.</param>
        /// <returns>A float from 0 to 1 representing the ratio of straights to turns with the window centered at the path location. 
        /// Note, if the window exceeds the path it is cropped to the valid region. If the resulting window size is less than one, a -1 is returned.</returns>
        public float SpeedAgilityRatio(int pathIndex, int halfWindowSize = 2)
        {
            int numberOfStraights = 0;
            int numberOfTurns = 0;
            int startIndex = Math.Max(0,pathIndex-halfWindowSize);
            int endIndex = Math.Min(pathIndex + halfWindowSize, PathLength - 2);
            int windowSize = endIndex - startIndex + 1;
            if (windowSize == 1) return TurtlePath[startIndex] == 'S' ? 1 : 0;
            if (windowSize <= 0) return -1;
            for(int i=startIndex; i <= endIndex; i++)
            {
                if (TurtlePath[i] == 'S') numberOfStraights++;
                else numberOfTurns++;
            }
            return (float)numberOfStraights / (float)windowSize;
        }
    }
}