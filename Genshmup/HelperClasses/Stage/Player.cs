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
        public Point Position { get => _position; set => _position = value; }
        private Point _position;

        public Player()
        {
            
        }

        public void Move(Direction dir)
        {
            switch (dir)
            {
                case Direction.Up:
                    _position.Y -= 10;
                    break;
                case Direction.Down:
                    _position.Y += 10;
                    break;
                case Direction.Left:
                    _position.X -= 10;
                    break;
                case Direction.Right:
                    _position.X += 10;
                    break;
            }
        }
    }
}
