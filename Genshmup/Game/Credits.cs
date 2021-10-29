using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Genshmup.HelperClasses;
using System.Drawing;
using System.Reflection;
using System.Drawing.Text;

namespace Genshmup.Game
{
    public class Credits : Screen
    {
        private bool dialog = true;
        private bool paused = false;
        private int selectedIndex = 0;

        Image BGI;

        private int keysleep = 0;

        protected string dialogString = "";
        protected Dialog parsedDialog;
        protected DialogElement currentElement = new(ElementType.TextLine, "", "");
        protected int condition = 0;

        private readonly Font titlefont;

        public Credits()
        {
            titlefont = new Font(ResourceLoader.LoadFont(Assembly.GetExecutingAssembly(), "menu.ttf") ?? new FontFamily(GenericFontFamilies.Serif), 24);

            parsedDialog = DialogParser.Parse(new StreamReader(ResourceLoader.LoadResource(null, "Ending.dlg") ?? Stream.Null).ReadToEnd());

            BGI = Image.FromStream(ResourceLoader.LoadResource(null, "sucroseWW.png") ?? Stream.Null);
        }

        public override void Init()
        {
            
        }

        public override void Render(Graphics g)
        {
            try
            {
                // Draw BG
                g.Clear(Color.Black);
                g.DrawImage(BGI, 0, 0, 480, 360);

                // Dialog
                if (dialog)
                {
                    g.FillRectangle(new SolidBrush(DanmakuGraphics.ColorFromUInt(0xA0000000)), new Rectangle(5, 205, 150, 35));
                    g.DrawString(currentElement.Author,
                        new Font(titlefont.FontFamily, 16, FontStyle.Regular), Brushes.White, new Point(20, 210));

                    g.FillRectangle(new SolidBrush(DanmakuGraphics.ColorFromUInt(0xA0000000)), new Rectangle(5, 245, 470, 110));
                    if (currentElement.Type == ElementType.TextLine || currentElement.Type == ElementType.Conditional)
                        g.DrawString(dialogString, new Font(titlefont.FontFamily, 16, FontStyle.Regular),
                            Brushes.White, new Rectangle(20, 260, 440, 90),
                            new StringFormat() { Trimming = StringTrimming.EllipsisWord });
                    else if (currentElement.Type == ElementType.BigTextLine)
                        g.DrawString(dialogString, new Font(titlefont.FontFamily, 72, FontStyle.Bold), Brushes.White, new Point(20, 260));
                    else if (currentElement.Type == ElementType.Prompt)
                    {
                        g.DrawString(dialogString, new Font(titlefont.FontFamily, 16, FontStyle.Regular),
                            Brushes.White, new Rectangle(20, 260, 440, 90),
                            new StringFormat() { Trimming = StringTrimming.EllipsisWord });
                        for (int i = 0; i < (currentElement.Choices == null ? 0 : currentElement.Choices.Length); i++)
                        {
                            g.FillRectangle(new SolidBrush(DanmakuGraphics.ColorFromUInt(0xA0000000)), new Rectangle(245, 205 - i * 40, 230, 40));
                            if (condition == i)
                                g.FillRectangle(new SolidBrush(DanmakuGraphics.ColorFromUInt(0xA0FFFFFF)), new Rectangle(245, 205 - i * 40, 230, 40));
#pragma warning disable CS8602
                            g.DrawString(currentElement.Choices[i], new Font(titlefont.FontFamily, 12, FontStyle.Regular), Brushes.White, new Point(190, 210 - i * 40));
#pragma warning restore CS8602
                        }
                    }
                    g.DrawString("Enter to continue", new Font(titlefont.FontFamily, 10, FontStyle.Regular), Brushes.White, new Point(360, 340));
                }

                // Pause Screen
                if (paused)
                {
                    using (SolidBrush brush = new(Color.FromArgb(180, 0, 0, 0)))
                    {
                        g.FillRectangle(brush, 0, 0, 480, 360);
                    }
                    g.DrawString("Resume", new Font(titlefont, FontStyle.Bold), Brushes.Gray, new Point(80, 80));
                    g.DrawString("Menu", new Font(titlefont, FontStyle.Bold), Brushes.Gray, new Point(80, 160));
                    g.DrawString("Exit", new Font(titlefont, FontStyle.Bold), Brushes.Gray, new Point(80, 200));
                    if (selectedIndex == 0) g.DrawString("Resume", titlefont, Brushes.White, new Point(80, 80));
                    else if (selectedIndex == 1) g.DrawString("Menu", titlefont, Brushes.White, new Point(80, 160));
                    else g.DrawString("Exit", titlefont, Brushes.White, new Point(80, 200));
                }
            }
            catch
            {
                base.Render(g);
            }
        }

        public override LogicExit Logic(string[] events)
        {
            List<string> ev = new();
            for (int i = 0; i < events.Length; i++)
                if (!ev.Contains(events[i])) ev.Add(events[i]);
            events = ev.ToArray();

            // Dialog
            if (dialog)
            {
                if (dialogString != currentElement.Content)
                {
                    dialogString += currentElement.Content[dialogString.Length];
                }
            }
            //Key sleep (dialog before needs to work tho)
            if (keysleep - 1 >= 0)
            {
                keysleep--;
                return LogicExit.Nothing;
            }
            if (dialog)
            {
                if (dialogString == currentElement.Content)
                {
                    if (currentElement.Type == ElementType.Prompt)
                    {
                        foreach (string eventName in events)
                        {
                            switch (eventName)
                            {
                                case "Up":
                                    SoundPlayer.PlaySound("select.wav", true);
#pragma warning disable CS8602
                                    condition = (condition + 1) % currentElement.Choices.Length;
                                    keysleep = 10;
                                    break;
                                case "Down":
                                    SoundPlayer.PlaySound("select.wav", true);
                                    condition = ((condition - 1) >= 0 ? condition : currentElement.Choices.Length) - 1;
#pragma warning restore CS8602
                                    keysleep = 10;
                                    break;
                            }
                        }
                    }
                    if (events.Contains("Enter") || events.Contains("Z") || events.Contains("Y"))
                    {
                        SoundPlayer.PlaySound("enter.wav", true);
                        if (!parsedDialog.MoveNext())
                        {
                            dialog = false;
                            return LogicExit.Nothing;
                        }
                        dialogString = "";
                        currentElement = parsedDialog.Current;
                        while ((currentElement.Type == ElementType.Conditional && currentElement.Condition != condition) || (currentElement.Type == ElementType.HardcodeEvent))
                        {
                            if (currentElement.Type == ElementType.HardcodeEvent)
                            {
                                switch (currentElement.Content)
                                {
                                    case "ClrScr":
                                        BGI = new Func<Bitmap>(() => { Bitmap b = new(1, 1); b.SetPixel(0, 0, Color.Black); return b; })();
                                        break;
                                    case "Unlock":
                                        if (!File.Exists("./g.dat"))
                                            File.WriteAllBytes("./g.dat", new byte[] { 0x01, 0x00, 0x00, 0x00 });
                                        break;
                                }
                            }
                            if (!parsedDialog.MoveNext())
                            {
                                dialog = false;
                                return LogicExit.Nothing;
                            }
                            currentElement = parsedDialog.Current;
                        }
                        keysleep = 20;
                    }
                    if (events.Contains("S"))
                    {
                        SoundPlayer.PlaySound("enter.wav", true);
                        SoundPlayer.DisposeAll();
                        SoundPlayer.PlaySoundLoop("st1.flac");
                        dialog = false;
                        return LogicExit.Nothing;
                    }
                }
                else
                {
                    if (events.Contains("Enter") || events.Contains("Z") || events.Contains("Y"))
                    {
                        dialogString = currentElement.Content;
                    }
                }
            }
            else
            {
                _nextScreen = 0;
                return LogicExit.ScreenChange;
            }

            // Paused Screen
            if (paused)
            {
                foreach (string eventName in events)
                {
                    switch (eventName)
                    {
                        case "Up":
                            SoundPlayer.PlaySound("select.wav", true);
                            selectedIndex = ((selectedIndex - 1) >= 0 ? selectedIndex : 3) - 1;
                            keysleep = 10;
                            break;
                        case "Down":
                            SoundPlayer.PlaySound("select.wav", true);
                            selectedIndex = (selectedIndex + 1) % 3;
                            keysleep = 10;
                            break;
                        case "Enter":
                            SoundPlayer.PlaySound("enter.wav", true);
                            if (selectedIndex == 0)
                                paused = false;
                            else if (selectedIndex == 1)
                            {
                                _nextScreen = 0;
                                return LogicExit.ScreenChange;
                            }
                            else
                                return LogicExit.CloseApplication;
                            break;
                    }
                }
                return LogicExit.Nothing;
            }

            // Execute controls
            foreach (string eventName in events)
            {
                switch (eventName)
                {
                    case "Escape":
                        selectedIndex = 0;
                        SoundPlayer.PlaySound("enter.wav", true);
                        paused = !paused;
                        return LogicExit.Nothing; // Overwrite base handler
                }
            }

            return base.Logic(events);
        }
    }
}
