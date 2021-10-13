using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genshmup.HelperClasses;
using System.Drawing;
using System.Reflection;
using System.Drawing.Text;

namespace Genshmup.Game
{
    public class Stage1 : Stage
    {
        private Player player = new Player();

        public Stage1()
        {
            
        }
        public override void Init()
        {

        }

        public override void Dispose()
        {
            SoundPlayer.DisposeAll();
            base.Dispose();
        }

        public override void Render(Graphics g)
        {
            try
            {
                // Draw BG
                g.Clear(Color.Black);

                // Sidebar

                // Danmaku
                Rectangle CR = new Rectangle(0, 0, 240, 360);
                
            }
            catch
            {
                base.Render(g);
            }
        }

        public override LogicExit Logic(string[] events)
        {
            foreach (string eventName in events)
            {
                switch (eventName)
                {
                    case "Up":

                        break;
                    case "Down":

                        break;
                    case "Left":
                        
                        break;
                    case "Right":
                        
                        break;
                    case "Enter":
                    case "Z":
                    case "Y":
                        SoundPlayer.PlaySound("enter.wav", true);
                        break;
                    case "Escape":
                        SoundPlayer.PlaySound("enter.wav", true);
                        break;
                }
            }
            return base.Logic(events);
        }
    }
}
