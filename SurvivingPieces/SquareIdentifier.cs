using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurvivingPieces
{
    public class SquareIdentifier
    {
        public char File
        {
            get;
            private set;
        }
        public char Rank
        {
            get;
            private set;
        }
        public SquareIdentifier(string sq)
            : this(sq[0], sq[1])
        {
        }
        public SquareIdentifier(char file, char rank)
        {
            File = file;
            Rank = rank;
            if (!(File >= 'a' && File <= 'h' && Rank >= '1' && Rank <= '8'))
            {
                throw new Exception("Invalid square identifier");
            }
        }
        public override string ToString()
        {
            return File.ToString() + Rank.ToString();
        }
    }
}
