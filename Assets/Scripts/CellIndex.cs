using System;

namespace Reversi
{
    [Serializable]
    public struct CellIndex
    {
        public static readonly char[] RowNames = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
        public static readonly char[] ColNames = { '8', '7', '6', '5', '4', '3', '2', '1' };
        public static readonly (int row, int col)[] Directions = 
        {
            (0, 1), (0, -1), (1, 0), (-1, 0),
            (1, 1), (-1, -1), (1, -1), (-1, 1)
        };
        public int Row;
        public int Col;

        public CellIndex(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public CellIndex(CellIndex cellIndex)
        {
            Row = cellIndex.Row;
            Col = cellIndex.Col;
        }
        public override string ToString()
        {
            return $"{RowNames[Row]}{ColNames[Col]}";
        }
    }
}