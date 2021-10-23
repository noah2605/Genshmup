using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace Genshmup.HelperClasses
{
    // Bullet with a single Direction Vector (instead of a stage managed vector field
    public class SubBullet
    {
        private Point position;
        public Point Position { get => position; set => position = value; }

        public SubBullet(Point position)
        {
            Position = position;
            direction = Vector2.Zero;
        }

        private Vector2 direction;
        public Vector2 Direction { get => direction; set => direction = value; }

        public SubBullet(Point position, Vector2 direction)
        {
            Position = position;
            Direction = direction;
        }

        public SubBullet()
        {
            position = new Point(0, 0);
            direction = Vector2.Zero;
        }
    }
}
