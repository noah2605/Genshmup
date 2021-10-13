using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Text;
using Genshmup.HelperClasses;
using System.Drawing;
using System.Reflection;

namespace Genshmup.Game
{
    public class Menu : Screen
    {
        public bool KeepAspectRatio { get; set; } = false;
        private int SelectedIndex { get; set; }
        private int SelectedSettingsIndex { get; set; }
        private float TitleFontSize { get; set; }
        private float ItemFontSize { get; set; }

        private bool settings;

        private Font itemfont;
        private Font titlefont;
        private StringFormat sf;

        private List<Image> renderedList;
        private IEnumerator<Image> rendered;

        Point[] noisePoints = new Point[8];

        private string[] MenuItems =
        {
            "Play",
            "Settings",
            "Exit"
        };

        private (string, bool, int)[] SettingItems =
        {
            ("BGM Volume", false, 100),
            ("SFX Volume", false, 100),
            // ("Keep Aspect Ratio", true, 0),
            ("Back", true, 0)
        };

        public Menu()
        {
            SelectedIndex = 0;

            TitleFontSize = 36;
            ItemFontSize = 22;
            titlefont = new Font(ResourceLoader.LoadFont(Assembly.GetExecutingAssembly(), "menu.ttf") ?? new FontFamily(GenericFontFamilies.Serif), TitleFontSize);
            itemfont = new Font(ResourceLoader.LoadFont(Assembly.GetExecutingAssembly(), "menu.ttf") ?? new FontFamily(GenericFontFamilies.Serif), ItemFontSize);
            
            sf = new();
            sf.LineAlignment = StringAlignment.Near;
            sf.Trimming = StringTrimming.Word;

            renderedList = new List<Image>();
            rendered = renderedList.GetEnumerator();
        }
        public override void Init()
        {
            SoundPlayer.PlaySoundLoop("menu.flac");
            for (int i = 0; i < 30; i++)
                renderedList.Add(GenerateNoise());
            rendered = renderedList.GetEnumerator();
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
                // g.Clear(Color.Black);
                g.DrawImage(rendered.Current ?? renderedList.First(), new Rectangle(0, 0, 480, 360));
                if (!rendered.MoveNext()) rendered.Reset();

                // Title
                sf.Alignment = StringAlignment.Center;
                g.DrawString(settings ? "Settings" : "Genshmup", titlefont, 
                    new SolidBrush(DanmakuGraphics.ColorFromUInt(0xFFFFFFFF)), new Point(240, 0), sf);

                // Menu Items
                if (!settings)
                {
                    for (int i = 0; i < MenuItems.Length; i++)
                    {
                        if (SelectedIndex == i)
                        {
                            g.DrawString(MenuItems[i], itemfont, Brushes.White, new Point(80, 80 + i * 40), sf);
                            g.DrawString(MenuItems[i], new Font(itemfont, FontStyle.Bold), 
                                new SolidBrush(DanmakuGraphics.ColorFromUInt(0x7FFFFFFF)), new Point(80, 80 + i * 40), sf);
                        }
                        else g.DrawString(MenuItems[i], itemfont, Brushes.Gray, new Point(80, 80 + i * 40), sf);
                    }
                }
                else
                {
                    sf.Alignment = StringAlignment.Near;
                    for (int i = 0; i < SettingItems.Length; i++)
                    {
                        if (SelectedSettingsIndex == i)
                        {
                            g.DrawString(SettingItems[i].Item1, itemfont, Brushes.White, new Point(20, 80 + i * 40), sf);
                            g.DrawString(SettingItems[i].Item1, new Font(itemfont, FontStyle.Bold), 
                                new SolidBrush(DanmakuGraphics.ColorFromUInt(0x7FFFFFFF)), new Point(20, 80 + i * 40), sf);
                        }
                        else g.DrawString(SettingItems[i].Item1, itemfont, Brushes.Gray, new Point(20, 80 + i * 40), sf);
                    }
                    sf.Alignment = StringAlignment.Far;
                    for (int i = 0; i < SettingItems.Length - 1; i++) // Ignore "Back"
                    {
                        if (SelectedSettingsIndex == i)
                        {
                            g.DrawString(ItemToString(SettingItems[i].Item2, SettingItems[i].Item3), itemfont, 
                                Brushes.White, new Point(360, 80 + i * 40), sf);
                            g.DrawString(ItemToString(SettingItems[i].Item2, SettingItems[i].Item3), new Font(itemfont, FontStyle.Bold),
                                new SolidBrush(DanmakuGraphics.ColorFromUInt(0x7FFFFFFF)), new Point(360, 80 + i * 40), sf);
                        }
                        else g.DrawString(ItemToString(SettingItems[i].Item2, SettingItems[i].Item3), itemfont,
                            Brushes.Gray, new Point(360, 80 + i * 40), sf);
                    }
                }
            }
            catch
            {
                base.Render(g);
            }
        }

        private string ItemToString(bool type, int value)
        {
            if (value == 0) return "Off";
            if (type) return "On";
            else return Convert.ToString(value);
        }

        private Image GenerateNoise() { 
            int maxX = 100;
            int maxY = 100;
            Bitmap bmp = new Bitmap(maxX, maxY);
            Random rng = new Random(DateTime.Now.Millisecond);
            

            if (Point.Empty.Equals(noisePoints[0]))
                for (int i = 0; i < noisePoints.Length; i++)
                    noisePoints[i] = new Point(rng.Next(0, maxX), rng.Next(0, maxY));
            else
                for (int i = 0; i < noisePoints.Length; i++)
                    noisePoints[i] = new Point(
                        Math.Max(0, Math.Min(maxX, noisePoints[i].X + rng.Next(-maxX/10, maxX/10))), 
                        Math.Max(0, Math.Min(maxY, noisePoints[i].Y + rng.Next(-maxY/10, maxY/10)))
                    );

            for (int y = 0; y < maxY; y++)
            {
                for (int x = 0; x < maxX; x++)
                {
                    Color c = Color.FromArgb(
                        (int)Math.Sqrt(noisePoints
                            .Select(p => (p.X - x) * (p.X - x) + (p.Y - y) * (p.Y - y))
                            .OrderBy(x => x)
                            .ElementAt(0)
                        )
                    );
                    bmp.SetPixel(x, y, Color.FromArgb(255, c));
                }
            }

            return bmp;
        }

        public override LogicExit Logic(string[] events)
        {
            foreach (string eventName in events)
            {
                switch (eventName)
                {
                    case "Up":
                        SoundPlayer.PlaySound("select.wav", true);
                        if (!settings) SelectedIndex = (SelectedIndex - 1) < 0 ? MenuItems.Length - 1 : (SelectedIndex - 1);
                        else SelectedSettingsIndex = (SelectedSettingsIndex - 1) < 0 ? SettingItems.Length - 1 : (SelectedSettingsIndex - 1);
                        return LogicExit.Nothing;
                    case "Down":
                        SoundPlayer.PlaySound("select.wav", true);
                        if (!settings) SelectedIndex = (SelectedIndex + 1) % MenuItems.Length;
                        else SelectedSettingsIndex = (SelectedSettingsIndex + 1) % SettingItems.Length;
                        return LogicExit.Nothing;
                    case "Left":
                        if (!SettingItems[SelectedSettingsIndex].Item2)
                            SettingItems[SelectedSettingsIndex].Item3 = Math.Max(0, SettingItems[SelectedSettingsIndex].Item3 - 1);
                        SoundPlayer.Volume = SettingItems[0].Item3 / 100.0f;
                        SoundPlayer.SFXVolume = SettingItems[1].Item3 / 100.0f;
                        break;
                    case "Right":
                        if (!SettingItems[SelectedSettingsIndex].Item2)
                            SettingItems[SelectedSettingsIndex].Item3 = Math.Min(100, SettingItems[SelectedSettingsIndex].Item3 + 1);
                        SoundPlayer.Volume = SettingItems[0].Item3 / 100.0f;
                        SoundPlayer.SFXVolume = SettingItems[1].Item3 / 100.0f;
                        break;
                    case "Enter":
                    case "Z":
                    case "Y":
                        SoundPlayer.PlaySound("enter.wav", true);
                        if (!settings)
                        {
                            switch (SelectedIndex)
                            {
                                case 0:
                                    _nextScreen = 1;
                                    return LogicExit.ScreenChange;
                                case 1:
                                    settings = !settings;
                                    break;
                                case 2:
                                    return LogicExit.CloseApplication;
                            }
                        }
                        else
                        {
                            switch (SelectedSettingsIndex)
                            {
                                case 2:
                                    settings = false;
                                    break;
                                default:
                                    if (SettingItems[SelectedSettingsIndex].Item2)
                                        SettingItems[SelectedSettingsIndex].Item3 = (SettingItems[SelectedSettingsIndex].Item3 + 1) % 2;
                                    break;
                            }
                        }
                        break;
                    case "Escape":
                        SoundPlayer.PlaySound("enter.wav", true);
                        if (settings) settings = false;
                        else return LogicExit.CloseApplication;
                        return LogicExit.Nothing; // Override base handler
                }
            }
            return base.Logic(events);
        }
    }
}
