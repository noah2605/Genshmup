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

        private int _lives;
        public int Lives { get => _lives; set => _lives = value; }

        public Rectangle Rect { get => new(_position.X, _position.Y, _size.Width, _size.Height); }

        public Player()
        {
            _size = new Size(32, 32);
            _position = new Point(240 - _size.Width / 2, 180 - _size.Height / 2);
        }

        public void Move(Direction dir, int step)
        {
            switch (dir)
            {
                case Direction.Up:
                    _position.Y -= step;
                    break;
                case Direction.Down:
                    _position.Y += step;
                    break;
                case Direction.Left:
                    _position.X -= step;
                    break;
                case Direction.Right:
                    _position.X += step;
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
