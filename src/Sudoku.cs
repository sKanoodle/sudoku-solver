using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver
{
    class Sudoku
    {
        public SudokuTile[] Tiles = new SudokuTile[81];

        public HashSet<int> UnsolvedTiles;

        private Sudoku() { }

        public Sudoku(int[] numbers)
        {
            if (numbers.Length != 81) throw new ArgumentException();

            UnsolvedTiles = Enumerable.Range(0, 81).ToHashSet();
            for ( int i = 0; i < 81; i++)
            {
                int number = numbers[i];
                if (number == 0)
                    Tiles[i] = new SudokuTile(GetRowForTile(i), GetColForTile(i), GetNinthForTile(i));
                else
                {
                    Tiles[i] = new SudokuTile(GetRowForTile(i), GetColForTile(i), GetNinthForTile(i), number);
                    UnsolvedTiles.Remove(i);
                }
            }

        }

        public IEnumerable<SudokuTile> GetRow(int row)
        {
            if (row < 0 || row > 8) throw new IndexOutOfRangeException();

            return Tiles.Skip(row * 9).Take(9);
        }

        public IEnumerable<SudokuTile> GetCol(int col)
        {
            if (col < 0 || col > 8) throw new IndexOutOfRangeException();

            for (int i = 0; i < 9; i++)
                yield return Tiles[col + i * 9];
        }

        public IEnumerable<SudokuTile> GetNinth(int ninth)
        {
            if (ninth < 0 || ninth > 8) throw new IndexOutOfRangeException();

            int startRow = ninth / 3 * 3;
            int startCol = ninth % 3 * 3;

            for (int row = startRow; row < startRow + 3; row++)
                for (int col = startCol; col < startCol + 3; col++)
                {
                    yield return Tiles[row * 9 + col];
                }
        }

        private int GetRowForTile(int tile)
        {
            if (tile < 0 || tile > 80) throw new IndexOutOfRangeException();

            return tile / 9;
        }

        private int GetColForTile(int tile)
        {
            if (tile < 0 || tile > 80) throw new IndexOutOfRangeException();

            return tile % 9;
        }

        private int GetNinthForTile(int tile)
        {
            int col = GetColForTile(tile) / 3;
            int row = GetRowForTile(tile) / 3;

            return row * 3 + col;
        }

        public void Draw()
        {
            Console.SetCursorPosition(0, 0);
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    int tileIndex = row * 9 + col;

                    if (Tiles[tileIndex].HasValue)
                    {
                        if (Tiles[tileIndex].IsGiven)
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                        else
                            Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(Tiles[tileIndex].Value);
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                    }
                    else Console.Write(" ");

                    if (col == 2 | col == 5)
                        Console.Write("|");
                    if (col == 8)
                        Console.WriteLine();
                }

                if (row == 2 || row == 5)
                    Console.WriteLine("---+---+---");
            }
        }

        public Sudoku Clone()
        {
            Sudoku result = new Sudoku();
            result.UnsolvedTiles = UnsolvedTiles.ToHashSet();
            Array.Copy(Tiles, result.Tiles, Tiles.Length);
            return result;
        }
    }

    class SudokuTile
    {
        public int Row { get; }
        public int Col { get; }
        public int Ninth { get; }

        public bool IsGiven { get; } = false;
        public int Value { get; private set; }

        public bool HasValue => IsGiven || Value != 0;

        public SudokuTile(int row, int col, int ninth, int value)
            : this(row, col, ninth)
        {
            IsGiven = true;
            Value = value;
        }

        public SudokuTile(int row, int col, int ninth)
        {
            Row = row;
            Col = col;
            Ninth = ninth;
        }

        public SudokuTile WithValue(int value)
        {
            if (IsGiven) throw new Exception("this tile is given and cannot be changed!");
            SudokuTile result = new SudokuTile(Row, Col, Ninth);
            result.Value = value;
            return result;
        }

        private static readonly int[] AllValues = Enumerable.Range(1, 9).ToArray();

        public IEnumerable<int> CalculatePossibleValues(Sudoku sudoku)
        {
            if (HasValue) throw new Exception("already has value");

            var takenValues = sudoku.GetRow(Row)
                .Concat(sudoku.GetCol(Col))
                .Concat(sudoku.GetNinth(Ninth))
                .Where(t => t.HasValue)
                .Select(t => t.Value);

            return AllValues.Except(takenValues);
        }
    }
}
