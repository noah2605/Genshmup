using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Text;
using System.Threading.Tasks;
using Genshmup.HelperClasses;
using System.Drawing;
using System.Reflection;
using System.Numerics;
using System.IO;

namespace Genshmup.Game
{
    public class Stage2 : Stage
    {
        private int keysleep = 20;
        private int deathprotection = 60;

        private readonly Player player = new();
        private readonly Boss boss = new();

        private readonly List<Image> renderedList;
        private IEnumerator<Image> rendered;
        readonly Point[] noisePoints = new Point[8];

        private readonly int movementSpeed = 10;
        private readonly int movementSpeedShifting = 2;
        private bool shifting = false;

        private readonly Image Kakbrazeus;
        private readonly Image Dvalin;

        private readonly Font titlefont;
        private readonly StringFormat sf;

        private Rectangle CR;

        private Point[][] bulletPositions;
        private readonly Image bulletAtlas;
        private readonly Rectangle[] bulletElements;
        private readonly int[] bulletSpeeds;
        private readonly int[] bulletDamages;
        private readonly int[] bulletCooldowns;
        private int[] _bulletCooldowns;

        private Point[][] bulletPositionsBoss;
        private readonly Rectangle[] bulletElementsBoss;
        private Func<Vector2, Vector2>[] vectorFields;
        private Vector2 EpiCenter;



        private bool gameover = false;
        private bool dialog = true;
        private int selectedIndex = 0;

        private string dialogString = "";
        private Dialog parsedDialog;
        private DialogElement currentElement = new(ElementType.TextLine, "", "");
        private int condition = 0;

        public Stage2()
        {
            Kakbrazeus = Image.FromStream(ResourceLoader.LoadResource(null, "kakbrazeus.png") ?? System.IO.Stream.Null);
            Dvalin = Image.FromStream(ResourceLoader.LoadResource(null, "ganyu.png") ?? System.IO.Stream.Null);
            titlefont = new Font(ResourceLoader.LoadFont(Assembly.GetExecutingAssembly(), "menu.ttf") ?? new FontFamily(GenericFontFamilies.Serif), 36);
            sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            };

            bulletPositions = new Point[3][];
            for (int i = 0; i < bulletPositions.Length; i++)
            {
                bulletPositions[i] = Array.Empty<Point>();
            }
            bulletPositionsBoss = new Point[3][];
            for (int i = 0; i < bulletPositionsBoss.Length; i++)
            {
                bulletPositionsBoss[i] = Array.Empty<Point>();
            }
            bulletElements = new Rectangle[] {
                new Rectangle(0, 0, 32, 32),
                new Rectangle(32, 0, 32, 32),
                new Rectangle(64, 0, 32, 32)
            };

            bulletElementsBoss = new Rectangle[] {
                new Rectangle(0, 32, 32, 32),
                new Rectangle(32, 32, 32, 32),
                new Rectangle(64, 32, 32, 32)
            };

            bulletAtlas = Image.FromStream(ResourceLoader.LoadResource(null, "bullets1.png") ?? Stream.Null);
            bulletSpeeds = new int[] { 20, 10, 5 };
            bulletDamages = new int[] { 2, 600, 1200 };
            bulletCooldowns = new int[] { 5, 600, 1200 };
            _bulletCooldowns = new int[3];
            Array.Fill(_bulletCooldowns, 0);
            CR = new Rectangle(0, 0, 480, 360);

            renderedList = new();
            rendered = renderedList.GetEnumerator();

            player.Lives = 3;
            parsedDialog = DialogParser.Parse(new StreamReader(ResourceLoader.LoadResource(null, "Stage2.dlg") ?? Stream.Null).ReadToEnd());

            vectorFields = new Func<Vector2, Vector2>[]
            {
                Straight,
                Spiral,
                Target
            };

            EpiCenter = new Vector2(boss.Position.X, boss.Position.Y);
        }

        public override void Init()
        {
            SoundPlayer.PlaySound("stage_intro.wav", true);

            boss.Position = new Point(224, 40);
            boss.Health = 10000;
            for (int i = 0; i < 30; i++)
                renderedList.Add(GenerateNoise());
            rendered = renderedList.GetEnumerator();
        }

        private Image GenerateNoise()
        {
            int maxX = 100;
            int maxY = 100;
            Bitmap bmp = new(maxX, maxY);
            Random rng = new(DateTime.Now.Millisecond);


            if (Point.Empty.Equals(noisePoints[0]))
                for (int i = 0; i < noisePoints.Length; i++)
                    noisePoints[i] = new Point(rng.Next(0, maxX), rng.Next(0, maxY));
            else
                for (int i = 0; i < noisePoints.Length; i++)
                    noisePoints[i] = new Point(
                        Math.Max(0, Math.Min(maxX, noisePoints[i].X + rng.Next(-maxX / 10, maxX / 10))),
                        Math.Max(0, Math.Min(maxY, noisePoints[i].Y + rng.Next(-maxY / 10, maxY / 10)))
                    );

            for (int y = 0; y < maxY; y++)
            {
                for (int x = 0; x < maxX; x++)
                {
                    Color c = Color.FromArgb(
                        ((int)Math.Sqrt(noisePoints
                            .Select(p => (p.X - x) * (p.X - x) + (p.Y - y) * (p.Y - y))
                            .OrderBy(x => x)
                            .ElementAt(0)
                        ) << 8)
                    );
                    bmp.SetPixel(x, y, Color.FromArgb(255, c));
                }
            }

            return bmp;
        }

        public override void Render(Graphics g)
        {
            try
            {
                // Draw BG
                // g.Clear(Color.Black);
                g.DrawImage(rendered.Current ?? renderedList.First(), new Rectangle(0, 0, 480, 360));
                if (!rendered.MoveNext()) rendered.Reset();

                // Figures
                g.DrawImage(Kakbrazeus, player.Position);
                if (!dialog && deathprotection != 0)
                {
                    g.DrawEllipse(new Pen(Color.BlueViolet, 2.0f), player.Rect);
                }
                g.DrawImage(Dvalin, boss.Position);

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
                    return;
                }

                // Danmaku
                DanmakuGraphics.RenderAtlas(g, bulletAtlas, bulletElements, bulletPositions);

                g.DrawString($"{_bulletCooldowns[1]}, {_bulletCooldowns[2]}\n{boss.Health}, {player.Lives}", titlefont, Brushes.White, new Point(240, 180), sf);

                DanmakuGraphics.RenderAtlas(g, bulletAtlas, bulletElementsBoss, bulletPositionsBoss);

                // Game Over
                if (gameover)
                {
                    Bitmap Bmp = new(480, 360);
                    using (Graphics gfx = Graphics.FromImage(Bmp))
                    using (SolidBrush brush = new(Color.FromArgb(180, 0, 0, 0)))
                    {
                        gfx.FillRectangle(brush, 0, 0, 480, 360);
                    }
                    g.DrawImage(Bmp, new Point(0, 0));
                    g.DrawString("Retry", new Font(titlefont, FontStyle.Bold), Brushes.Gray, new Point(80, 120));
                    g.DrawString("Menu", new Font(titlefont, FontStyle.Bold), Brushes.Gray, new Point(80, 200));
                    if (selectedIndex == 0) g.DrawString("Retry", titlefont, Brushes.White, new Point(80, 120));
                    if (selectedIndex == 1) g.DrawString("Menu", titlefont, Brushes.White, new Point(80, 200));
                }
            }
            catch
            {
                base.Render(g);
            }
        }

        private void Shoot(int type)
        {
            if (!(_bulletCooldowns[type] >= bulletCooldowns[type])) return;
            SoundPlayer.PlaySound(type == 0 ? "shot.wav" : (type == 1 ? "elem_1.wav" : "ult_1.wav"), true);
            List<Point> pres = bulletPositions[type].ToList();
            pres.Add(player.Position);
            bulletPositions[type] = pres.ToArray();
            _bulletCooldowns[type] = 0;
        }

        private void BossShoot(int type)
        {
            List<Point> pres = bulletPositionsBoss[type].ToList();
            pres.Add(new Point(boss.Position.X + 3, boss.Position.Y + 3));
            bulletPositionsBoss[type] = pres.ToArray();
        }

        public override LogicExit Logic(string[] events)
        {
            List<string> ev = new();
            for (int i = 0; i < events.Length; i++)
                if (!ev.Contains(events[i])) ev.Add(events[i]);
            events = ev.ToArray();

            EpiCenter = new Vector2(boss.Position.X, boss.Position.Y);

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
#pragma warning restore CS8602
                                    keysleep = 10;
                                    break;
                                case "Down":
                                    SoundPlayer.PlaySound("select.wav", true);
#pragma warning disable CS8602
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
                        while (currentElement.Type == ElementType.Conditional && currentElement.Condition != condition)
                        {
                            if (!parsedDialog.MoveNext())
                            {
                                dialog = false;
                                return LogicExit.Nothing;
                            }
                            currentElement = parsedDialog.Current;
                        }
                        keysleep = 20;
                    }
                }
                else
                {
                    if (events.Contains("Enter") || events.Contains("Z") || events.Contains("Y"))
                    {
                        dialogString = currentElement.Content;
                    }
                }

                return base.Logic(events);
            }
            else
                deathprotection = Math.Max(deathprotection - 1, 0);

            // Game Over Screen
            if (gameover)
            {
                foreach (string eventName in events)
                {
                    switch (eventName)
                    {
                        case "Up":
                            SoundPlayer.PlaySound("select.wav", true);
                            selectedIndex = ((selectedIndex - 1) >= 0 ? selectedIndex : 2) - 1;
                            keysleep = 20;
                            break;
                        case "Down":
                            SoundPlayer.PlaySound("select.wav", true);
                            selectedIndex = (selectedIndex + 1) % 2;
                            keysleep = 20;
                            break;
                        case "Enter":
                        case "Z":
                        case "Y":
                            _nextScreen = selectedIndex == 0 ? 1 : 0;
                            return LogicExit.ScreenChange;
                    }
                }
                return LogicExit.Nothing;
            }

            // Check for Player Collisions
            if (deathprotection == 0)
                for (int i1 = 0; i1 < bulletPositionsBoss.Length; i1++)
                {
                    Point[] pa = bulletPositionsBoss[i1];
                    for (int i = 0; i < pa.Length; i++)
                    {
                        Point ta = pa[i];
                        Rectangle t = new(ta, new Size(16, 16));
                        if (player.Hitbox.IntersectsWith(t))
                        {
                            SoundPlayer.PlaySound("death.wav", true);
                            player.Lives--;
                            List<Point> bpb = bulletPositionsBoss[i1].ToList();
                            bpb.Remove(pa[i]);
                            bulletPositionsBoss[i1] = bpb.ToArray();
                            deathprotection = 60;
                        }
                    }
                }

            // Check for Game Over Condition
            if (player.Lives <= 0)
            {
                SoundPlayer.PlaySound("stage_failed.wav", true);
                gameover = true;
            }

            // Move boss (TO BE MADE UH.... BETTER)
            if (new Random().Next(0, 20) == 0)
            {
                boss.Position = new Point(boss.Position.X + new Random().Next(-10, 10), boss.Position.Y + new Random().Next(-10, 10));
                BossShoot(0);
                boss.Bound(CR);
            }

            // She ded
            if (boss.Health <= 0)
            {
                SoundPlayer.PlaySound("boss_death.wav", true);
                _nextScreen = 2;
                return LogicExit.ScreenChange;
            }

            // Advance Boss Bullets
            for (int t = 0; t < bulletPositionsBoss.Length; t++)
            {
                for (int i = 0; i < bulletPositionsBoss[t].Length; i++)
                {
                    Vector2 cr = vectorFields[t](new Vector2(bulletPositionsBoss[t][i].X, bulletPositionsBoss[t][i].Y));
                    bulletPositionsBoss[t][i] = new Point((int)cr.X, (int)cr.Y);
                }
            }

            for (int t = 0; t < bulletPositions.Length; t++)
            {
                if (_bulletCooldowns[t] < bulletCooldowns[t])
                    _bulletCooldowns[t]++;
                for (int i = 0; i < bulletPositions[t].Length; i++)
                {
                    bulletPositions[t][i].Y -= bulletSpeeds[t];
                    if (bulletPositions[t][i].X - boss.Position.X != 0)
                        bulletPositions[t][i].X -= (int)Math.Pow(t + 1, 2) * (bulletPositions[t][i].X - boss.Position.X) / Math.Abs(bulletPositions[t][i].X - boss.Position.X);
                    if (boss.Rect.IntersectsWith(new Rectangle(bulletPositions[t][i], new Size(16, 16))))
                    {
                        List<Point> pres = bulletPositions[t].ToList();
                        pres.Remove(bulletPositions[t][i]);
                        bulletPositions[t] = pres.ToArray();
                        boss.Health -= bulletDamages[t];
                        i--;
                        continue;
                    }
                    if (!CR.Contains(bulletPositions[t][i]))
                    {
                        List<Point> pres = bulletPositions[t].ToList();
                        pres.Remove(bulletPositions[t][i]);
                        bulletPositions[t] = pres.ToArray();
                        i--;
                        continue;
                    }
                }
            }

            foreach (string eventName in events)
            {
                switch (eventName)
                {
                    case "Up":
                        player.Move(Direction.Up, shifting ? movementSpeedShifting : movementSpeed);
                        break;
                    case "Down":
                        player.Move(Direction.Down, shifting ? movementSpeedShifting : movementSpeed);
                        break;
                    case "Left":
                        player.Move(Direction.Left, shifting ? movementSpeedShifting : movementSpeed);
                        break;
                    case "Right":
                        player.Move(Direction.Right, shifting ? movementSpeedShifting : movementSpeed);
                        break;
                    case "Enter":
                    case "Z":
                    case "Y":
                        Shoot(0);
                        break;
                    case "E":
                        Shoot(1);
                        break;
                    case "Q":
                        Shoot(2);
                        break;
                    case "Escape":
                        SoundPlayer.PlaySound("enter.wav", true);
                        break;
                    case "ShiftKey":
                    case "RShiftKey":
                        shifting = true;
                        break;
                }
            }
            if (!events.Contains("ShiftKey") && !events.Contains("RShiftKey"))
                shifting = false;

            player.Bound(CR);
            return base.Logic(events);
        }

        private Vector2 Straight(Vector2 pos)
        {
            return new Vector2(0, 5) + pos;
        }

        private Vector2 Spiral(Vector2 pos)
        {
            pos -= EpiCenter;

            Polar polar = new(pos.Length(), (float)Math.Atan2(pos.Y, pos.X));
            polar.radius += 1;
            polar.angle += 0.1f;

            return EpiCenter + new Vector2(polar.radius * (float)Math.Cos(polar.angle), polar.radius * (float)Math.Sin(polar.angle));
        }

        private Vector2 Target(Vector2 pos)
        {
            Vector2 ppos = new(player.Position.X + 16, player.Position.Y + 16);
            Vector2 bpos = new(boss.Position.X + 16, boss.Position.Y + 16);
            return pos + new Vector2(0, 1) + (ppos - bpos) / 40;
        }
    }
}
