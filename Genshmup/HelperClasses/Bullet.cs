using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;

namespace Genshmup.HelperClasses
{
    public class Bullet
    {
        public BulletType BulletType { get; set; }
        public Complex Position { get; set; }
        public Vector2 Direction { get; set; }
        public double Angle { get => Math.Atan2(Direction.Y, Direction.X); }
    }
}
