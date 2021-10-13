using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genshmup.HelperClasses
{
    public class Stage : Screen
    {
        public override void Render(Graphics g)
        {
            base.Render(g);
        }

        public override LogicExit Logic(string[] events)
        {
            return base.Logic(events);
        }

        public override void Dispose()
        {
            SoundPlayer.DisposeAll();
            base.Dispose();
        }
    }
}
