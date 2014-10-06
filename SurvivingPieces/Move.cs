using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurvivingPieces
{
    public class Move
    {
        public SquareIdentifier from;
        public SquareIdentifier to;
        public bool ep;
        public bool castle;

        public Move()
        {
            ep = false;
            castle = false;
        }
    }
}
