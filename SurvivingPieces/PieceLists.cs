using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurvivingPieces
{
    public class PieceLists
    {
        private static int KeyFromSquare(char file, char rank)
        {
            return ((file - 'a') * 8) + (rank - '1');
        }
        private static int KeyFromSquare(SquareIdentifier sq)
        {
            return KeyFromSquare(sq.File, sq.Rank);
        }


        private readonly static char[] pawnRanks = new[] { '2', '7' };
        private readonly static SquareIdentifier[] kingSquares = new[] { 
        new SquareIdentifier("e8"),
        new SquareIdentifier("e1"),
        };


        public static bool IsPawn(int k)
        {
            return pawnRanks.Contains((char)((k % 8) + '1'));
        }

        public static bool IsKing(int k)
        {
            return kingSquares.Select(si => KeyFromSquare(si)).Contains(k);
        }

        int[] _origToCurrent;
        int[] _currentToOrig;

        private void InitialiseStartingPieces()
        {
            var ranks = new[] { '1', '2', '7', '8' };
            for (char file = 'a'; file <= 'h'; ++file)
            {
                foreach (char rank in ranks)
                {
                    var si = new SquareIdentifier(file, rank);
                    var key = KeyFromSquare(si);
                    _currentToOrig[key] = key;
                    _origToCurrent[key] = key;
                }
            }
        }

        public bool Occupied(SquareIdentifier sq)
        {
            return _currentToOrig[KeyFromSquare(sq)] >= 0;
        }


        public int MaybeCapture(SquareIdentifier sq, int time)
        {
            var atCurrent = _currentToOrig[KeyFromSquare(sq)];

            if (atCurrent >= 0)
            {
                Debug.Assert(!IsKing(atCurrent));

                _origToCurrent[atCurrent] = -1;
                _currentToOrig[KeyFromSquare(sq)] = -1;

                _tod.Add(GetStringRep(atCurrent), time);
            }

            return atCurrent;
        }

        public int Move(SquareIdentifier from, SquareIdentifier to)
        {
            var atCurrent = _currentToOrig[KeyFromSquare(from)];

            System.Diagnostics.Debug.Assert(atCurrent >= 0);
            System.Diagnostics.Debug.Assert(_origToCurrent[atCurrent] == KeyFromSquare(from));

            var atDest = _currentToOrig[KeyFromSquare(to)];
            System.Diagnostics.Debug.Assert(atDest < 0);

            _currentToOrig[KeyFromSquare(from)] = -1;
            _currentToOrig[KeyFromSquare(to)] = atCurrent;

            _origToCurrent[atCurrent] = KeyFromSquare(to);

            return atCurrent;
        }

        public PieceLists()
        {
            _origToCurrent = new int[64];
            _currentToOrig = new int[64];
            for (int i = 0; i < 64; ++i)
            {
                _currentToOrig[i] = -1;
                _origToCurrent[i] = -1;
            }
            InitialiseStartingPieces();
        }
        
        private Dictionary<string, int> _tod = new Dictionary<string, int>();

        public IReadOnlyDictionary<string, int> TimeOfDeath
        {
            get
            {
                return _tod;
            }
        }


        public List<string> GetSurvivors()
        {
            var l = _currentToOrig
                .Where(i => i >= 0)
                .Select(i => GetStringRep(i)).ToList();

            if (!l.Contains("e8") || !l.Contains("e1"))
                throw new InvalidOperationException("King gone!");

            return l;

        }

        private static string GetStringRep(int i)
        {
            Debug.Assert(i >= 0);

            char[] stuff = new char[2];
            stuff[0] = (char)('a' + (i / 8));
            stuff[1] = (char)('1' + (i % 8));

            return new String(stuff);

            throw new NotImplementedException();
        }
    }
}
