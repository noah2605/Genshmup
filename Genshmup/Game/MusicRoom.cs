using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genshmup.HelperClasses;

namespace Genshmup.Game
{
    public class MusicRoom : Screen
    {
        public override void Render(Graphics g)
        {
            g.Clear(Color.Black);
            g.DrawString("Not implemented yet", new Font(FontFamily.GenericSerif, 20), Brushes.White, new Point(0, 0));
        }
    }
}
