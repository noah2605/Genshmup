using System.Drawing;
using System.Drawing.Text;
using System.Reflection;
using Genshmup.HelperClasses;

namespace Genshmup.Game
{
    public class MusicRoom : Screen
    {
        private readonly Font titlefont;

        private int selectedIndex = 0;
        private int playing = -1;

        private readonly (string, string)[] trackList = {
            ("Qilin's Protection", "st1.flac"),
            ("Caelestina Necarisse Terminorum", "st2.flac"),
            ("Alchimia Nigrea ~ Prohibita", "st3.flac"),
            ("Deep Ice ~ Heaven's Cowbell", "menu.flac")
        };

        public MusicRoom()
        {
            titlefont = new Font(ResourceLoader.LoadFont(Assembly.GetExecutingAssembly(), "menu.ttf") ?? new FontFamily(GenericFontFamilies.Serif), 24);
        }

        public override void Render(Graphics g)
        {
            g.Clear(Color.Black);
            g.DrawString("Music Room", titlefont, Brushes.White, new Point(240, 0), new StringFormat() { Alignment = StringAlignment.Center });

            for (int i = 0; i < trackList.Length; i++)
            {
                g.DrawString($"{i + 1}. " + trackList[i].Item1, new Font(titlefont.FontFamily, 14f), Brushes.Gray, new Point(10, 80 + i * 40));
                if (selectedIndex == i) g.DrawString($"{i + 1}. " + trackList[i].Item1, new Font(titlefont.FontFamily, 14f), Brushes.White, new Point(10, 80 + i * 40));
                if (playing == i) g.DrawRectangle(new Pen(Color.DarkRed, 3), new Rectangle(438, 80 + i * 40, 20, 20));
                else g.DrawPolygon(new Pen(Color.DarkGreen, 3), new Point[] { new Point(438, 80 + i * 40), new Point(458, 90 + 40 * i), new Point(438, 100 + 40 * i) });
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
                        selectedIndex = (selectedIndex - 1 < 0 ? trackList.Length : selectedIndex) - 1;
                        break;
                    case "Down":
                        SoundPlayer.PlaySound("select.wav", true);
                        selectedIndex = (selectedIndex + 1) % trackList.Length;
                        break;
                    case "Enter":
                    case "Y":
                    case "Z":
                        SoundPlayer.DisposeAll();
                        if (playing == selectedIndex)
                            playing = -1;
                        else
                        {
                            SoundPlayer.PlaySoundLoop(trackList[selectedIndex].Item2);
                            playing = selectedIndex;
                        }
                        break;
                    case "Escape":
                        _nextScreen = 0;
                        return LogicExit.ScreenChange;
                }

            return base.Logic(events);
        }
    }
}
