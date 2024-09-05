using System;

namespace Reversi
{
    [Serializable]
    public struct CellIndex
    {
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
    }
}