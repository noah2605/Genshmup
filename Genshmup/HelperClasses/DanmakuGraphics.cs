using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Genshmup.HelperClasses
{
    public static class DanmakuGraphics
    {
        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));
            if (pen == null)
                throw new ArgumentNullException(nameof(pen));

            using GraphicsPath path = RoundedRect(bounds, cornerRadius);
            graphics.DrawPath(pen, path);
        }

        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));
            if (brush == null)
                throw new ArgumentNullException(nameof(brush));

            using GraphicsPath path = RoundedRect(bounds, cornerRadius);
            graphics.FillPath(brush, path);
        }

        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new(diameter, diameter);
            Rectangle arc = new(bounds.Location, size);
            GraphicsPath path = new();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }
            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
        public static void RenderAtlas(Graphics g, Image atlas, Rectangle[] elements, Point[][] positions)
        {
            if (elements.Length != positions.Length) throw new ArgumentException("Array of Elements not mappable to Array of Positions");
            for (int i = 0; i < elements.Length; i++)
                for (int ii = 0; ii < positions[i].Length; ii++)
                    g.DrawImage(atlas, new Rectangle(positions[i][ii], new Size(16, 16)), elements[i], GraphicsUnit.Pixel);
        }

        public static Point[] Parallelogram(Point fulcrum, float width, float height, float angle)
        {
            List<Point> points = new();
            points.Add(new Point(fulcrum.X + (int)(width / 2 * Math.Cos(angle)), fulcrum.Y + (int)(width / 2 * Math.Sin(angle))));
            points.Add(new Point(fulcrum.X - (int)(width / 2 * Math.Cos(angle)), fulcrum.Y - (int)(width / 2 * Math.Sin(angle))));
            points.Add(new Point(points[1].X + (int)(height * Math.Cos(angle - Math.PI / 2)), points[1].Y + (int)(height * Math.Sin(angle - Math.PI / 2))));
            points.Add(new Point(points[0].X + (int)(height * Math.Cos(angle - Math.PI / 2)), points[0].Y + (int)(height * Math.Sin(angle - Math.PI / 2))));
            return points.ToArray();
        }

        public static Point[] Parallelogram(Point[] parallelogram, Point newfulcrum, float width, float height, float angleincre)
        {
            int dx = parallelogram[1].X - parallelogram[2].X;
            int dy = parallelogram[1].Y - parallelogram[2].Y;
            float angle = MathF.Atan2(dy, dx) - MathF.PI / 2;
            return Parallelogram(newfulcrum, width, height, angle + angleincre);
        }

        public static bool PolygonContains(Point[] vertices, Point p)
        {
            bool collision = false;
            int n;
            for (int c = 0; c < vertices.Length; c++)
            {
                n = (c + 1) % vertices.Length;
                Point vc = vertices[c];
                Point vn = vertices[n];
                if (((vc.Y >= p.Y && vn.Y < p.Y) || (vc.Y < p.Y && vn.Y >= p.Y)) &&
                    (p.X < (vn.X - vc.X) * (p.Y - vc.Y) / (vn.Y - vc.Y) + vc.X))
                    collision = !collision;
            }
            return collision;
        }

        public static Color ColorFromUInt(uint h)
        {
            return Color.FromArgb((int)h);
        }
    }
}