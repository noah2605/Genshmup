using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Genshmup.HelperClasses
{
    public static class DanmakuGraphics
    {
        public static void RenderAtlas(Graphics g, Image atlas, Rectangle[] elements, Point[][] positions)
        {
            if (elements.Length != positions.Length) throw new ArgumentException("Array of Elements not mappable to Array of Positions");
            for (int i = 0; i < elements.Length; i++)
                g.DrawImage(atlas, positions[i], elements[i], GraphicsUnit.Pixel);
        }
        public static Color ColorFromUInt(uint h)
        {
            return Color.FromArgb((int)h);
        }
    }
}
