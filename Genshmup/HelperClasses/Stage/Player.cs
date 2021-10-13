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
        public Size Size { get => _size; set => _size = value; }
        private Size _size;

        public Player()
        {
            _size = new Size(32, 32);
            _position = new Point(240 - _size.Width/2, 180 - _size.Height/2);
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

        public void Bound(Rectangle rect)
        {
            Position = new Point(
                Math.Min(Math.Max(Position.X, rect.X), rect.Width + rect.X - Size.Width),
                Math.Min(Math.Max(Position.Y, rect.Y), rect.Height + rect.Y - Size.Height)
            );
        }
    }
}
