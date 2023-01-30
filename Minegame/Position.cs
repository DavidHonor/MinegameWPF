using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minegame
{
    class Position
    {
        public double left { get; set; }
        public double top { get; set; }

        public Position(int left, int top)
        {
            this.left = left;
            this.top = top;
        }

        public bool PositionMatch(Position ? other)
        {
            if(left == other?.left && top == other?.top) return true;
            return false;
        }
    }
}
