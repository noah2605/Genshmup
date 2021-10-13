using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Text;
using System.Threading.Tasks;
using Genshmup.HelperClasses;
using System.Drawing;
using System.Reflection;

namespace Genshmup.Game
{
    public class Menu : Screen
    {
        private int SelectedIndex { get; set; }
        private int SelectedSettingsIndex { get; set; }
        private float FontSize { get; set;  }

        private bool settings;

        private Font font;
        private StringFormat sf;

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
            ("Keep Aspect Ratio", true, 0)
        };

        public Menu()
        {
            SelectedIndex = 0;
            FontSize = 36;
            font = new Font(ResourceLoader.LoadFont(Assembly.GetExecutingAssembly(), "menu.ttf") ?? new FontFamily(GenericFontFamilies.Serif), FontSize);
            sf = new();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Near;
            sf.Trimming = StringTrimming.Word;
        }
        public override void Init()
        {
            SoundPlayer.PlaySoundLoop("menu.flac");
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
                // Clear Screen
                g.Clear(Color.Black);

                // Title
                g.DrawString(settings ? "Settings" : "Genshmup", font, 
                    new SolidBrush(DanmakuGraphics.ColorFromUInt(0xFFFFFFFF)), new Point(240, 0), sf);

                // Menu Items
                if (!settings)
                {
                    for (int i = 0; i < MenuItems.Length; i++)
                    {
                        if (SelectedIndex == i)
                        {
                            g.DrawString(MenuItems[i], font, Brushes.White, new Point(80, 80 + i * 40), sf);
                            g.DrawString(MenuItems[i], new Font(font, FontStyle.Bold), 
                                new SolidBrush(DanmakuGraphics.ColorFromUInt(0x7FFFFFFF)), new Point(80, 80 + i * 40), sf);
                        }
                        else g.DrawString(MenuItems[i], font, Brushes.Gray, new Point(80, 80 + i * 40), sf);
                    }
                }
                else
                {
                    for (int i = 0; i < SettingItems.Length; i++)
                    {
                        if (SelectedSettingsIndex == i)
                        {
                            g.DrawString(SettingItems[i].Item1, font, Brushes.White, new Point(80, 80 + i * 40), sf);
                            g.DrawString(MenuItems[i], new Font(font, FontStyle.Bold), 
                                new SolidBrush(DanmakuGraphics.ColorFromUInt(0x7FFFFFFF)), new Point(80, 80 + i * 40), sf);
                        }
                        else g.DrawString(MenuItems[i], font, Brushes.Gray, new Point(80, 80 + i * 40), sf);
                    }
                }
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
                        SoundPlayer.PlaySound("select.wav", true);
                        if (!settings) SelectedIndex = (SelectedIndex - 1) < 0 ? MenuItems.Length - 1 : (SelectedIndex - 1);
                        else SelectedSettingsIndex = (SelectedSettingsIndex - 1) < 0 ? SettingItems.Length - 1 : (SelectedSettingsIndex - 1);
                        return LogicExit.Nothing;
                    case "Down":
                        SoundPlayer.PlaySound("select.wav", true);
                        if (!settings) SelectedIndex = (SelectedIndex + 1) % MenuItems.Length;
                        else SelectedSettingsIndex = (SelectedSettingsIndex + 1) % SettingItems.Length;
                        return LogicExit.Nothing;
                    case "Enter":
                    case "Z":
                    case "Y":
                        SoundPlayer.PlaySound("enter.wav");
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
                        break;
                    case "Escape":
                        SoundPlayer.PlaySound("enter.wav", true);
                        if (settings) settings = false;
                        else return LogicExit.CloseApplication;
                        break;
                }
            }
            return base.Logic(events);
        }
    }
}
