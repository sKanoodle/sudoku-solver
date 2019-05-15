using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SudokuSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            // https://www.websudoku.com/?level=1&set_id=2217386028
            Sudoku easy = new Sudoku(new[] {
                0,5,0,  0,0,0,  0,0,0,
                9,0,7,  0,0,0,  2,0,0,
                2,0,4,  0,6,0,  3,1,9,

                3,0,0,  8,5,0,  0,0,6,
                8,4,9,  0,1,0,  5,3,2,
                6,0,0,  0,2,4,  0,0,1,

                5,1,8,  0,7,0,  9,0,4,
                0,0,2,  0,0,0,  7,0,3,
                0,0,0,  0,0,0,  0,5,0,
            });

            // https://www.websudoku.com/?level=3&set_id=3240158230
            Sudoku hard = new Sudoku(new[] {
                0,0,0,  0,0,6,  0,0,5,
                9,7,0,  1,8,0,  0,0,6,
                0,0,0,  0,0,0,  1,0,0,

                2,0,8,  0,0,1,  0,9,0,
                4,0,7,  0,0,0,  6,0,1,
                0,6,0,  5,0,0,  4,0,8,

                0,0,9,  0,0,0,  0,0,0,
                8,0,0,  0,4,9,  0,6,3,
                6,0,0,  3,0,0,  0,0,0,
            });

            // https://www.websudoku.com/?level=4&set_id=8255281410
            Sudoku evil = new Sudoku(new[] {
                0,0,0,  0,0,0,  0,0,0,
                0,6,0,  0,0,7,  2,5,0,
                0,0,4,  1,5,0,  3,0,0,

                0,2,0,  0,9,5,  6,0,0,
                0,4,0,  0,0,0,  0,9,0,
                0,0,3,  2,4,0,  0,7,0,

                0,0,7,  0,6,8,  1,0,0,
                0,3,6,  5,0,0,  0,8,0,
                0,0,0,  0,0,0,  0,0,0,
            });

            //SolveWithMetrics(easy);
            //SolveWithMetrics(hard);
            SolveWithMetrics(evil);

            Console.ReadKey();
        }

        private static void SolveWithMetrics( Sudoku sudoku )
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var result = Solve(sudoku);
            watch.Stop();
            result.Draw();
            Console.WriteLine(watch.Elapsed);
            Console.WriteLine();
        }

        public static Sudoku Solve( Sudoku sudoku, int depth = 0 )
        {
            while (true)
            {
                //sudoku.Draw();

                if (sudoku.UnsolvedTiles.Count == 0)
                {
                    return sudoku; // solved
                }

                // try easiest method of just looking for tiles with single possible value
                switch (Solve1(sudoku))
                {
                    case SolveTryState.None: throw new ArgumentException();
                    case SolveTryState.FoundAtLeastOne: continue;
                    case SolveTryState.NoNewFindings: break;
                    case SolveTryState.ImpossibleSolution: return null;
                    default: throw new NotImplementedException();
                }

                // now get into recursion with guessing tiles and seeing if the sudoku is then solvable
                Sudoku result = null;
                if ((result = Solve2(sudoku, depth)) != null)
                    return result;

                return null; // not solvable
            }
        }

        private static SolveTryState Solve1(Sudoku sudoku)
        {
            bool didFindOne = false;
            var tilesToCheck = sudoku.UnsolvedTiles.ToArray();
            foreach (int tile in tilesToCheck)
            {
                var possibleValues = sudoku.Tiles[tile].CalculatePossibleValues(sudoku).ToArray();
                if (possibleValues.Length < 1)
                    return SolveTryState.ImpossibleSolution;
                if (possibleValues.Length < 2)
                {
                    sudoku.Tiles[tile] = sudoku.Tiles[tile].WithValue(possibleValues[0]);
                    sudoku.UnsolvedTiles.Remove(tile);
                    didFindOne = true;
                }
            }
            return didFindOne ? SolveTryState.FoundAtLeastOne : SolveTryState.NoNewFindings;
        }

        private static Sudoku Solve2(Sudoku sudoku, int depth)
        {
            List<Task<Sudoku>> tasks = new List<Task<Sudoku>>();
            var tilesToCheck = sudoku.UnsolvedTiles.ToArray();
            foreach (int tile in tilesToCheck)
            {
                var possibleValues = sudoku.Tiles[tile].CalculatePossibleValues(sudoku).ToArray();
                foreach (int value in possibleValues)
                {
                    var sudoku2 = sudoku.Clone();

                    sudoku2.Tiles[tile] = sudoku2.Tiles[tile].WithValue(value);
                    sudoku2.UnsolvedTiles.Remove(tile);

                    if (depth == 0)
                        tasks.Add(Task.Run(() => Solve(sudoku2, depth + 1)));

                    else
                    {
                        Sudoku result;
                        if ((result = Solve(sudoku2, depth + 1)) != null)
                            return result;
                    }
                }
            }

            int i = 0;
            while (tasks.Count > 0)
            {
                int index = Task.WaitAny(tasks.ToArray());
                Console.WriteLine(i++);
                var task = tasks[index];
                tasks.RemoveAt(index);
                if (task.Result != null)
                    return task.Result;
            }
            return null;
        }

        enum SolveTryState { None, NoNewFindings, ImpossibleSolution, FoundAtLeastOne }
    }
}
