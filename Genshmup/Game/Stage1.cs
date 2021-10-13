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

        private Image Kakbrazeus;

        private Font titlefont;
        private StringFormat sf;

        private Rectangle CR;

        public Stage1()
        {
            Kakbrazeus = Image.FromStream(ResourceLoader.LoadResource(null, "kakbrazeus.png") ?? System.IO.Stream.Null);
            titlefont = new Font(ResourceLoader.LoadFont(Assembly.GetExecutingAssembly(), "menu.ttf") ?? new FontFamily(GenericFontFamilies.Serif), 36);
            sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Near;

            CR = new Rectangle(0, 0, 480, 360);
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

                // Dialog


                // Danmaku
                g.DrawImage(Kakbrazeus, player.Position);
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
                        player.Move(Direction.Up);
                        break;
                    case "Down":
                        player.Move(Direction.Down);
                        break;
                    case "Left":
                        player.Move(Direction.Left);
                        break;
                    case "Right":
                        player.Move(Direction.Right);
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

            player.Bound(CR);

            return base.Logic(events);
        }
    }
}
