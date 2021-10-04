using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genshmup.HelperClasses;
using System.Drawing;

namespace Genshmup.Game
{
    public class Menu : Screen
    {
        private int SelectedIndex { get; set; }

        public Menu()
        {
            SelectedIndex = 0;
        }

        public override void Render(Graphics g)
        {
            base.Render(g);
        }

        public override LogicExit Logic(string[] events)
        {
            foreach (string eventName in events)
            {
                switch (eventName)
                {
                    case "Up":
                        _nextScreen = 0;
                        return LogicExit.ScreenChange;
                }
            }
            return base.Logic(events);
        }
    }
}
