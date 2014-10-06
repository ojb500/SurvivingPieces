using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace SurvivingPieces
{
    class Program
    {
        private static IEnumerable<IEnumerable<string>> Games(TextReader tr)
        {
            List<string> moves = new List<string>();

            string s = "";
            for (; ; )
            {
                try
                {
                    s = tr.ReadLine();
                }
                catch (IOException)
                {
                    yield break;
                }
                if (s == null)
                {
                    yield return moves;
                    yield break;
                }
                s = s.Trim();
                if (s == "")
                {
                    yield return moves;
                    moves = new List<string>();
                }
                else
                {
                    var ms = s.Split(' ').Where(st => !st.EndsWith("."));
                    moves.AddRange(ms);
                }
            }
        }

        static void Main(string[] args)
        {
            Dictionary<string, int> _stats = new Dictionary<string, int>();

            var file = new System.IO.StreamReader(string.Join(" ", args));
            var db = Games(file);
            
            // Initialise stats
            for (char f = 'a'; f <= 'h'; f++)
            {
                for (char r = '1'; r <= '8'; r++)
                {
                    string s = string.Format("{0}{1}", f, r);
                    _stats.Add(s, 0);
                }
            }

            int nRejected = 0;
            int nGame = 0;
            foreach (var g in db)
            {
                nGame++;
                try
                {
                    var moves = new List<string>(g);

                    var plist = new PieceLists();

                    if (moves.Count < 5)
                        continue; // ignore ridiculously short or empty games

                    foreach (var m in moves)
                    {
                        var mv = ResolveMove(m);
                        
                        // make the move
                        if (mv.castle)
                        {
                            var rookFrom = GetRookStartSquare(mv);
                            var rookTo = GetRookEndSquare(mv);

                            plist.Move(rookFrom, rookTo);
                            var movedKing = plist.Move(mv.from, mv.to);
                            Debug.Assert(PieceLists.IsKing(movedKing));
                        }
                        else
                        {
                            if (mv.ep)
                            {
                                var epRank = mv.to.Rank == '6' ? '5' : '4';
                                var epPawnSquare = new SquareIdentifier(mv.to.File, epRank);

                                var captureResult = plist.MaybeCapture(epPawnSquare);
                                Debug.Assert(captureResult >= 0);
                            }
                            else
                            {
                                var captureResult = plist.MaybeCapture(mv.to);
                            }

                            plist.Move(mv.from, mv.to);
                        }
                    }

                    var survivors = plist.GetSurvivors();
                    foreach (var piece in survivors)
                    {
                        _stats[piece]++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception thrown in game {0}", nGame);
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine();
                    nRejected++;
                }
                if (nGame % 10000 == 0)
                {
                    Console.WriteLine("done {0} games", nGame);
                }
            }

            PrintStats(_stats);

            Console.WriteLine("{0} games could not be read and were ignored", nRejected);
            Console.ReadLine();
        }

        private static void PrintStats(Dictionary<string, int> _stats)
        {
            var orderedStats = _stats.OrderByDescending(kvp => kvp.Value).Where(kvp => kvp.Value > 0);
            foreach (var kvp in orderedStats)
            {
                Console.WriteLine("{0},{1}", kvp.Key, kvp.Value);
            }
        }

        private static SquareIdentifier GetRookEndSquare(Move mv)
        {
            var rank = mv.from.Rank;
            var file = (mv.to.File == 'c' ? 'd' : 'f');
            return new SquareIdentifier(file, rank);
        }

        private static SquareIdentifier GetRookStartSquare(Move mv)
        {
            var rank = mv.from.Rank;
            var file = (mv.to.File == 'c' ? 'a' : 'h');
            return new SquareIdentifier(file, rank);
        }

        private static Move ResolveMove(string sanMove)
        {
            // We get a move that looks like
            // Nc3d5
            // c4d3ep
            // e1g1         (White O-O)

            Move m = new Move();

            // Trim off the piece character
            char possiblePiece = sanMove[0];
            sanMove = sanMove.TrimStart('R', 'K', 'B', 'N', 'Q')
                .TrimEnd('+', '#', '!', '?');

            if (sanMove.EndsWith("ep"))
            {
                m.ep = true;
                sanMove = sanMove.Substring(0, sanMove.Length - 2);
            }

            // Pull out the coordinates
            var from = sanMove.Substring(0, 2);
            var to = sanMove.Substring(2, 2);

            m.from = new SquareIdentifier(from);
            m.to = new SquareIdentifier(to);

            // Detect castling (move string didn't start with {RNBKQ}, leaving us with pawn moves,
            //                  and the piece moves along the same rank)
            char[] pieces = { 'R', 'N', 'B', 'K', 'Q' };
            if (!pieces.Contains(possiblePiece) && m.from.Rank - m.to.Rank == 0)
            {
                m.castle = true;
            }

            return m;
        }
    }
}
