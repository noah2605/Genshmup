using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genshmup.HelperClasses;
using System.Reflection;
using System.Drawing.Text;

namespace Genshmup.Game
{
    public class StageSelect : Screen
    {
        private readonly Font titlefont;

        private int selectedIndex = 0;

        private readonly (string, string)[] stageList = {
            ("Stage 1 - Liyue Mountains", "ganyu.png"),
            ("Stage 2 - Mondstadt Cliffs", "dvalin.png"),
            ("Stage 3 - Dvalin's Nest", "sonjaboss.png"),
            ("Ending", "sucroseWW.png")
        };

        public StageSelect()
        {
            titlefont = new Font(ResourceLoader.LoadFont(Assembly.GetExecutingAssembly(), "menu.ttf") ?? new FontFamily(GenericFontFamilies.Serif), 24);
        }

        public override void Render(Graphics g)
        {
            g.Clear(Color.Black);
            g.DrawString("Stage Select", titlefont, Brushes.White, new Point(240, 0), new StringFormat() { Alignment = StringAlignment.Center });

            for (int i = 0; i < stageList.Length; i++)
            {
                Point ogn = new(10 + (i % 2) * 220, 40 + (i > 1 ? 140 : 0));
                g.DrawRoundedRectangle(new Pen(Color.Gray, 6f), new Rectangle(ogn, new Size(200, 100)), 18);
                g.DrawString(stageList[i].Item1, new Font(titlefont.FontFamily, 10f, FontStyle.Bold), Brushes.Gray, new Point(ogn.X + 100, ogn.Y + 100),
                    new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far });
                if (selectedIndex == i)
                {
                    g.DrawImage(Image.FromStream(ResourceLoader.LoadResource(null, stageList[i].Item2) ?? System.IO.Stream.Null), new Rectangle(ogn.X + 64, ogn.Y + 8, 72, 72));
                    g.DrawRoundedRectangle(new Pen(Color.White, 3f), new Rectangle(ogn, new Size(200, 100)), 18);
                    g.DrawString(stageList[i].Item1, new Font(titlefont.FontFamily, 10f), Brushes.White, new Point(ogn.X + 100, ogn.Y + 100),
                    new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far });
                }
            }

            g.DrawString("Escape to Return to Menu", new Font(titlefont.FontFamily, 14f), Brushes.White, new Point(240, 340));
        }

        public override LogicExit Logic(string[] events)
        {
            foreach (string ev in events)
                switch (ev)
                {
                    case "Up":
                        SoundPlayer.PlaySound("select.wav", true);
                        selectedIndex = selectedIndex - 2 < 0 ? (-(selectedIndex - 2) == 1 ? stageList.Length - 1 : stageList.Length - 2) : selectedIndex - 2;
                        break;
                    case "Down":
                        SoundPlayer.PlaySound("select.wav", true);
                        selectedIndex = (selectedIndex + 2) % stageList.Length;
                        break;
                    case "Left":
                        SoundPlayer.PlaySound("select.wav", true);
                        selectedIndex = (selectedIndex - 1 < 0 ? stageList.Length : selectedIndex) - 1;
                        break;
                    case "Right":
                        SoundPlayer.PlaySound("select.wav", true);
                        selectedIndex = (selectedIndex + 1) % stageList.Length;
                        break;
                    case "Enter":
                    case "Y":
                    case "Z":
                        _nextScreen = selectedIndex + 1;
                        return LogicExit.ScreenChange;
                    case "Escape":
                        _nextScreen = 0;
                        return LogicExit.ScreenChange;
                }

            return base.Logic(events);
        }
    }
}
