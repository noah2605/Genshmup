using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Genshmup.HelperClasses
{
    public class Boss
    {
        private Point _position;
        private Size _size;
        private int _health;

        public Point Position { get => _position; set => _position = value; }
        public Size Size { get => _size; set => _size = value; }
        public int Health {  get => _health; set => _health = value; }
        public Rectangle Rect { get => new(_position.X, _position.Y, _size.Width, _size.Height); }

        public Boss()
        {
            Position = new Point(0, 0);
            Size = new Size(32, 32);
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
