using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Genshmup.HelperClasses
{
    public class Player
    {
        public Point Position { get => new Point(_x, _y); }

        private int _x;
        private int _y;

        public Player()
        {
            
        }
    }
}
