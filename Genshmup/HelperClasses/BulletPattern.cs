using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace Genshmup.HelperClasses
{
    public class Pattern
    {
        private Func<Point, Vector2> vectorField;
        public Point Center { get; set; }

        public Vector2 VectorAt(Point pt)
        {
            return vectorField(pt);
        }

        public Pattern(Func<Point, Vector2> vectorField)
        {
            this.vectorField = vectorField;
        }
    }
}
