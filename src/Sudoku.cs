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

            foreach (var index in UnsolvedTiles.ToArray())
            {
                Tiles[index].SetInitialPossibleValues(this);
                if (Tiles[index].HasValue)
                    UnsolvedTiles.Remove(index);
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

        public bool Solve2()
        {
            bool foundOne = false;
            for (int i = 0; i < 9; i++)
            {
                if (EliminatePossibleValues(GetRow(i)))
                    foundOne = true;
                if (EliminatePossibleValues(GetCol(i)))
                    foundOne = true;
                if (EliminatePossibleValues(GetNinth(i)))
                    foundOne = true;
            }
            return foundOne;
        }

        /// <summary>
        /// looks if 2 tiles have the same 2 possible values, then all other tiles in that row/column/ninth cant have those 2 values and they can be eliminated as possibility.
        /// </summary>
        private bool EliminatePossibleValues(IEnumerable<SudokuTile> neighbours)
        {
            var tiles = neighbours.ToList();
            List<int> unsolved = tiles.Select((tile, index) => (tile, index)).Where(t => !t.tile.HasValue).Select(t => t.index).ToList();
            if (unsolved.Count < 3)
                return false;
            List<int> tilesWith2Possibles = unsolved.Where(i => tiles[i].PossibleValues.Count == 2).ToList();
            int index1 = 0, index2 = 0;
            bool found2TIles = false;
            foreach (int i1 in tilesWith2Possibles)
                foreach (int i2 in tilesWith2Possibles.Where(i => i != i1))
                    if (tiles[i1].PossibleValues.SetEquals(tiles[i2].PossibleValues))
                    {
                        index1 = i1;
                        index2 = i2;
                        found2TIles = true;
                    }
            if (!found2TIles)
                return false;

            unsolved = unsolved.Where(i => i != index1 && i != index2).ToList();
            bool foundOne = false;
            foreach (int index in unsolved)
            {
                tiles[index] = tiles[index].WithRemovedPossibleValues(tiles[index1].PossibleValues);
                if (tiles[index].HasValue)
                    foundOne = true;
            }
            return foundOne;
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
        public HashSet<int> PossibleValues { get; private set; }

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

        public void SetInitialPossibleValues(Sudoku sudoku)
        {
            var values = GetPossibleValues(sudoku).ToHashSet();
            if (values.Count == 1)
                Value = values.First();
            PossibleValues = values;
        }

        public SudokuTile WithUpdatedPossibleValues(Sudoku sudoku)
        {
            var toRemove = PossibleValues.Except(GetPossibleValues(sudoku)).ToArray();
            if (toRemove.Length < 1)
                return this;
            return WithRemovedPossibleValues(toRemove);
        }

        private IEnumerable<int> GetPossibleValues(Sudoku sudoku)
        {
            if (HasValue) throw new Exception("already has value");

            var takenValues = sudoku.GetRow(Row)
                .Concat(sudoku.GetCol(Col))
                .Concat(sudoku.GetNinth(Ninth))
                .Where(t => t.HasValue)
                .Select(t => t.Value);

            return AllValues.Except(takenValues);
        }

        public SudokuTile WithRemovedPossibleValue(int value) => WithRemovedPossibleValues(new[] { value });

        public SudokuTile WithRemovedPossibleValues(IEnumerable<int> values)
        {
            if (PossibleValues.Intersect(values).Count() == 0) // values are not in PossibleValues
                return this;
            SudokuTile result = new SudokuTile(Row, Col, Ninth);
            result.PossibleValues = PossibleValues.Except(values).ToHashSet();
            if (result.PossibleValues.Count < 1)
                throw new Exception("impossible situation encountered, there is no possible value here");
            if (result.PossibleValues.Count < 2)
                result.Value = result.PossibleValues.First();
            return result;
        }
    }
}
