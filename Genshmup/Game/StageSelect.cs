using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genshmup.HelperClasses;

namespace Genshmup.Game
{
    public class StageSelect : Screen
    {
        public override void Render(Graphics g)
        {
            g.Clear(Color.Black);
            g.DrawString("Not implemented yet", new Font(FontFamily.GenericSerif, 20), Brushes.White, new Point(0, 0));
        }

        public override LogicExit Logic(string[] events)
        {

            foreach (string ev in events)
                switch (ev)
                {
                    case "D1":
                        _nextScreen = 1;
                        return LogicExit.ScreenChange;
                    case "D2":
                        _nextScreen = 2;
                        return LogicExit.ScreenChange;
                    case "D3":
                        _nextScreen = 3;
                        return LogicExit.ScreenChange;
                    case "D4":
                        _nextScreen = 4;
                        return LogicExit.ScreenChange;
                }

            return base.Logic(events);
        }
    }
}
