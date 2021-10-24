using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Genshmup.HelperClasses;
using System.Drawing;
using System.Reflection;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.IO;

namespace Genshmup.Game
{
    public class Stage2 : Stage
    {
        private Image Ganyu;
        private Image Dvalin;

        private readonly Font titlefont;

        private SubBullet[] subBullets;

        private int pulse = 0;
        private int pulseskip = 3;

        private Func<Vector2, Vector2>[] vectorFields;

        public Stage2() : base()
        {
            Ganyu = Image.FromStream(ResourceLoader.LoadResource(null, "kakbrazeus.png") ?? Stream.Null);
            Dvalin = Image.FromStream(ResourceLoader.LoadResource(null, "ganyu.png") ?? Stream.Null);

            titlefont = new Font(ResourceLoader.LoadFont(Assembly.GetExecutingAssembly(), "menu.ttf") ?? new FontFamily(GenericFontFamilies.Serif), 24);
            bulletAtlas = Image.FromStream(ResourceLoader.LoadResource(null, "bullets2.png") ?? Stream.Null);

            bulletSpeeds = new int[] { 20, 10, 5 };
            bulletDamages = new int[] { 24, 300, 4000 };
            bulletCooldowns = new int[] { 5, 600, 1200 };

            parsedDialog = DialogParser.Parse(new StreamReader(ResourceLoader.LoadResource(null, "Stage2.dlg") ?? Stream.Null).ReadToEnd());

            vectorFields = new Func<Vector2, Vector2>[]
            {
                TightSpiral,
                SineDown,
                Spread
            };

            subBullets = Array.Empty<SubBullet>();
        }

        public override void Init()
        {
            base.Init();
            boss.Position = new Point(224, 40);
            boss.Health = 25000;
            for (int i = 0; i < 30; i++)
                renderedList.Add(GenerateNoise());
            rendered = renderedList.GetEnumerator();
            destinationBoss = boss.Position;
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
                g.DrawImage(Ganyu, player.Position);
                if (!dialog)
                {
                    switch (shieldType)
                    {
                        case 1:
                            g.DrawEllipse(new Pen(Color.BlueViolet, 2.0f), player.Rect);
                            break;
                        case 2:
                            g.DrawEllipse(new Pen(Color.DarkSeaGreen, 3.0f), player.Rect);
                            break;
                        case 3:
                            g.DrawEllipse(new Pen(Color.LightGoldenrodYellow, 4.0f), player.Rect);
                            break;
                    }
                }
                g.DrawImage(Dvalin, boss.Position);
                if (shifting)
                {
                    g.FillEllipse(Brushes.DarkRed, new Rectangle(player.Hitbox.X - 3, player.Hitbox.Y - 3, player.Hitbox.Width + 6, player.Hitbox.Height + 6));
                    g.FillEllipse(Brushes.Red, player.Hitbox);
                }

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

                // Debug Info
                // g.DrawString($"{_bulletCooldowns[1]}, {_bulletCooldowns[2]}\n{boss.Health}, {player.Lives}", titlefont, Brushes.White, new Point(240, 180), sf);

                DanmakuGraphics.RenderAtlas(g, bulletAtlas, bulletElementsBoss, bulletPositionsBoss);

                for (int i = 0; i < subBullets.Length; i++) g.DrawImage(bulletAtlas, new Rectangle(subBullets[i].Position, new Size(16, 16)), bulletElementsBoss[1], GraphicsUnit.Pixel);

                // Health Bars and other Info
                for (int i = 0; i < Math.Min(player.Lives, 3); i++) g.DrawImage(Heart, new Point(475 - 16 * (i + 1), 5));

                g.DrawString("Dvalin", new Font(titlefont.FontFamily, 11, FontStyle.Regular), Brushes.White, new Point(5, 5));
                g.DrawRectangle(Pens.White, new Rectangle(55, 5, 360, 20));
                g.FillRectangle(new LinearGradientBrush(new Rectangle(0, 0, 480, 20), Color.DarkBlue, Color.Teal, LinearGradientMode.ForwardDiagonal),
                    new Rectangle(56, 6, (int)(boss.Health * 358.0 / 25000.0), 18));

                g.DrawEllipse(new Pen(Color.White, _bulletCooldowns[2] == bulletCooldowns[2] ? 8f : 1f), new Rectangle(475 - 64, 355 - 64, 64, 64));
                g.DrawEllipse(new Pen(Color.White, _bulletCooldowns[1] == bulletCooldowns[1] ? 8f : 1f), new Rectangle(475 - 48 - 5 - 64, 355 - 48, 48, 48));

                // behold
                int sheight = (int)(_bulletCooldowns[2] * 64.0 / 1200.0);
                // the genius way to draw cropped cirlc
                // first we declare a Bitmap as buffer
                Bitmap b = new(64, 64);
                Graphics bg = Graphics.FromImage(b);
                // then we draw the circle
                bg.FillEllipse(new LinearGradientBrush(new Rectangle(0, 0, 64, 64), Color.White, Color.BlueViolet,
                    LinearGradientMode.ForwardDiagonal), new Rectangle(0, 0, 64, 64));
                // then we draw the erasing rectangle
                bg.FillRectangle(Brushes.Green, new Rectangle(0, 0, 64, 64 - sheight));
                b.MakeTransparent(Color.Green);
                // then we draw the image onto the main
                g.DrawImage(b, 475 - 64, 355 - 64);

                sheight = (int)(_bulletCooldowns[1] * 48.0 / 600.0);
                b = new(48, 48);
                bg = Graphics.FromImage(b);
                bg.FillEllipse(new LinearGradientBrush(new Rectangle(0, 0, 48, 48), Color.CadetBlue, Color.Teal,
                    LinearGradientMode.ForwardDiagonal), new Rectangle(0, 0, 48, 48));
                bg.FillRectangle(Brushes.Green, new Rectangle(0, 0, 48, 48 - sheight));
                b.MakeTransparent(Color.Green);
                g.DrawImage(b, 475 - 48 - 5 - 64, 355 - 48);

                g.DrawString("E", titlefont, Brushes.White, new Rectangle(475 - 48 - 5 - 64, 355 - 48, 48, 48), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                g.DrawString("Q", titlefont, Brushes.White, new Rectangle(475 - 64, 355 - 64, 64, 64), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                // Warning sign when phase changes

                // E and Q Effects

                // Game Over
                if (gameover)
                {
                    using (SolidBrush brush = new(Color.FromArgb(180, 0, 0, 0)))
                    {
                        g.FillRectangle(brush, 0, 0, 480, 360);
                    }
                    g.DrawString("Retry", new Font(titlefont, FontStyle.Bold), Brushes.Gray, new Point(80, 120));
                    g.DrawString("Menu", new Font(titlefont, FontStyle.Bold), Brushes.Gray, new Point(80, 200));
                    if (selectedIndex == 0) g.DrawString("Retry", titlefont, Brushes.White, new Point(80, 120));
                    else g.DrawString("Menu", titlefont, Brushes.White, new Point(80, 200));
                }

                // Pause Screen
                if (paused)
                {
                    using (SolidBrush brush = new(Color.FromArgb(180, 0, 0, 0)))
                    {
                        g.FillRectangle(brush, 0, 0, 480, 360);
                    }
                    g.DrawString("Resume", new Font(titlefont, FontStyle.Bold), Brushes.Gray, new Point(80, 80));
                    g.DrawString("Retry", new Font(titlefont, FontStyle.Bold), Brushes.Gray, new Point(80, 120));
                    g.DrawString("Menu", new Font(titlefont, FontStyle.Bold), Brushes.Gray, new Point(80, 160));
                    g.DrawString("Exit", new Font(titlefont, FontStyle.Bold), Brushes.Gray, new Point(80, 200));
                    if (selectedIndex == 0) g.DrawString("Resume", titlefont, Brushes.White, new Point(80, 80));
                    else if (selectedIndex == 1) g.DrawString("Retry", titlefont, Brushes.White, new Point(80, 120));
                    else if (selectedIndex == 2) g.DrawString("Menu", titlefont, Brushes.White, new Point(80, 160));
                    else g.DrawString("Exit", titlefont, Brushes.White, new Point(80, 200));
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
            if (type == 1)
            {
                protection = 500;
                shieldType = 2;
            }
            if (type == 2)
            {
                protection = 800;
                shieldType = 3;
                ClearRadius(50);
            }
        }

        private void BossShoot(int type, Point p)
        {
            List<Point> pres = bulletPositionsBoss[type].ToList();
            pres.Add(p);
            bulletPositionsBoss[type] = pres.ToArray();
        }

        public override LogicExit Logic(string[] events)
        {
            List<string> ev = new();
            for (int i = 0; i < events.Length; i++)
                if (!ev.Contains(events[i])) ev.Add(events[i]);
            events = ev.ToArray();

            _bulletCooldowns[2] = Math.Min(_bulletCooldowns[2], elementalEnergy);

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
                                    case "SwBg":
                                        Ganyu = Image.FromStream(ResourceLoader.LoadResource(null, "ganyu.png") ?? Stream.Null);
                                        Dvalin = Image.FromStream(ResourceLoader.LoadResource(null, "dvalin.png") ?? Stream.Null);
                                        break;
                                    case "StBgm":
                                        SoundPlayer.PlaySoundLoop("st2.flac");
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
                        Ganyu = Image.FromStream(ResourceLoader.LoadResource(null, "ganyu.png") ?? Stream.Null);
                        Dvalin = Image.FromStream(ResourceLoader.LoadResource(null, "dvalin.png") ?? Stream.Null);
                        SoundPlayer.PlaySound("enter.wav", true);
                        SoundPlayer.DisposeAll();
                        SoundPlayer.PlaySoundLoop("st2.flac");
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

                return base.Logic(events);
            }
            else
                protection = Math.Max(protection - 1, 0);
            if (protection == 0) shieldType = 0;

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
                            keysleep = 10;
                            break;
                        case "Down":
                            SoundPlayer.PlaySound("select.wav", true);
                            selectedIndex = (selectedIndex + 1) % 2;
                            keysleep = 10;
                            break;
                        case "Enter":
                            SoundPlayer.PlaySound("enter.wav", true);
                            _nextScreen = selectedIndex == 0 ? 2 : 0;
                            return LogicExit.ScreenChange;
                    }
                }
                return LogicExit.Nothing;
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
                            selectedIndex = ((selectedIndex - 1) >= 0 ? selectedIndex : 4) - 1;
                            keysleep = 10;
                            break;
                        case "Down":
                            SoundPlayer.PlaySound("select.wav", true);
                            selectedIndex = (selectedIndex + 1) % 4;
                            keysleep = 10;
                            break;
                        case "Enter":
                            SoundPlayer.PlaySound("enter.wav", true);
                            if (selectedIndex == 0)
                                paused = false;
                            else if (selectedIndex == 1)
                            {
                                _nextScreen = 2;
                                return LogicExit.ScreenChange;
                            }
                            else if (selectedIndex == 2)
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

            // Check for Player Collisions
            bool sbreak = false;
            for (int i1 = 0; i1 < bulletPositionsBoss.Length; i1++)
            {
                Point[] pa = bulletPositionsBoss[i1];
                for (int i = 0; i < pa.Length; i++)
                {
                    Point ta = pa[i];
                    Rectangle t = new(ta, new Size(16, 16));
                    if (player.Hitbox.IntersectsWith(t))
                    {
                        List<Point> bpb = bulletPositionsBoss[i1].ToList();
                        bpb.Remove(pa[i]);
                        bulletPositionsBoss[i1] = bpb.ToArray();
                        if (shieldType > 0)
                        {
                            // Only absorb a single hit if shield is from E
                            if (shieldType == 2)
                            {
                                shieldType = 0;
                                // Clear bullets in near distance
                                ClearRadius(20);
                            }
                            return LogicExit.Nothing;
                        }
                        SoundPlayer.PlaySound("death.wav", true);
                        player.Lives--;
                        shieldType = 1;
                        protection = 60;
                        sbreak = true;
                        break;
                    }
                }
                if (sbreak) break;
            }
            if (!sbreak)
                for (int i = 0; i < subBullets.Length; i++)
                {
                    Point ta = subBullets[i].Position;
                    Rectangle t = new(ta, new Size(16, 16));
                    if (player.Hitbox.IntersectsWith(t))
                    {
                        List<SubBullet> bpb = subBullets.ToList();
                        bpb.Remove(subBullets[i]);
                        subBullets = bpb.ToArray();
                        if (shieldType > 0)
                        {
                            if (shieldType == 2)
                            {
                                shieldType = 0;
                                ClearRadius(20);
                            }
                            return LogicExit.Nothing;
                        }
                        SoundPlayer.PlaySound("death.wav", true);
                        player.Lives--;
                        shieldType = 1;
                        protection = 60;
                        break;
                    }
                }

            // Check for Game Over Condition
            if (player.Lives <= 0)
            {
                selectedIndex = 0;
                SoundPlayer.PlaySound("stage_failed.wav", true);
                gameover = true;
            }

            // Move boss
            boss.Position = new Point(
                boss.Position.X + Math.Sign(destinationBoss.X - boss.Position.X) * (int)Math.Ceiling(Math.Abs(destinationBoss.X - boss.Position.X) / 12.0),
                boss.Position.Y + Math.Sign(destinationBoss.Y - boss.Position.Y) * (int)Math.Ceiling(Math.Abs(destinationBoss.Y - boss.Position.Y) / 12.0)
            );
            if (boss.Position == destinationBoss && new Random().Next(0, 140) == 0)
            {
                destinationBoss = new Point(
                    new Random().Next(0, 446),
                    new Random().Next(0, 100)
                );
            }
            boss.Bound(CR);

            // Boss shoot
            if (pulse > 0 || new Random().Next(0, 30) == 0)
            {
                if (pulseskip == 0)
                {
                    pulseskip = 3;
                    if (pulse == 0) pulse = 12;
                    else pulse--;
                    for (int i = 0; i < 6; i++)
                        BossShoot(0, new Point(
                            boss.Position.X + (int)(3.0 * Math.Cos(i * 2.0 * Math.PI / 6.0)),
                            boss.Position.Y + (int)(3.0 * Math.Sin(i * 2.0 * Math.PI / 6.0))
                        ));
                    if (boss.Health < 20000 && boss.Health >= 10000) BossShoot(1, boss.Position);
                    if (boss.Health < 10000)
                        for (int i = 0; i < 4; i++)
                            BossShoot(2, new Point(
                                boss.Position.X + (int)(3.0 * Math.Cos(i * Math.PI / 4.0 + Math.PI / 8.0)),
                                boss.Position.Y + (int)(3.0 * Math.Sin(i * Math.PI / 4.0 + Math.PI / 8.0))
                            ));
                }
                else pulseskip--;
            }

            // he ded
            if (boss.Health <= 0)
            {
                SoundPlayer.PlaySound("boss_death.wav", true);
                _nextScreen = 3;
                return LogicExit.ScreenChange;
            }

            // Advance Boss Bullets
            for (int t = 0; t < bulletPositionsBoss.Length; t++)
            {
                for (int i = 0; i < bulletPositionsBoss[t].Length; i++)
                {
                    if (t == 2)
                    {
                        if (Vector2.Distance(new Vector2(bulletPositionsBoss[t][i].X, bulletPositionsBoss[t][i].Y), new Vector2(boss.Position.X, boss.Position.Y)) >= 174.0f)
                        {
                            List<SubBullet> sbl = subBullets.ToList();
                            for (int s = 0; s < 4; s++)
                            {
                                SubBullet sb = new(bulletPositionsBoss[t][i], new Vector2(
                                    3f * (float)Math.Cos(s * Math.PI / 4f + Math.PI / 8.0f),
                                    3f * (float)Math.Sin(s * Math.PI / 4f + Math.PI / 8.0f)
                                ));
                                sbl.Add(sb);
                            }
                            subBullets = sbl.ToArray();
                            List<Point> pres = bulletPositionsBoss[t].ToList();
                            pres.Remove(bulletPositionsBoss[t][i]);
                            bulletPositionsBoss[t] = pres.ToArray();
                            i--;
                            continue;
                        }
                    }
                    Vector2 cr = vectorFields[t](new Vector2(bulletPositionsBoss[t][i].X, bulletPositionsBoss[t][i].Y));
                    bulletPositionsBoss[t][i] = new Point((int)cr.X, (int)cr.Y);
                    if (!new Rectangle(-240, 0, 960, 360).Contains(bulletPositionsBoss[t][i]))
                    {
                        List<Point> pres = bulletPositionsBoss[t].ToList();
                        pres.Remove(bulletPositionsBoss[t][i]);
                        bulletPositionsBoss[t] = pres.ToArray();
                        i--;
                        continue;
                    }
                }
            }
            for (int i = 0; i < subBullets.Length; i++)
            {
                subBullets[i].Position = new Point(
                    (int)(subBullets[i].Position.X + subBullets[i].Direction.X),
                    (int)(subBullets[i].Position.Y + subBullets[i].Direction.Y)
                );
                if (!CR.Contains(subBullets[i].Position))
                {
                    List<SubBullet> pres = subBullets.ToList();
                    pres.Remove(subBullets[i]);
                    subBullets = pres.ToArray();
                    i--;
                    continue;
                }
            }

            // Advance player bullets
            for (int t = 0; t < bulletPositions.Length; t++)
            {
                if (_bulletCooldowns[t] < bulletCooldowns[t])
                {
                    _bulletCooldowns[t]++;
                    if (_bulletCooldowns[t] == bulletCooldowns[t])
                    {
                        if (t == 1) SoundPlayer.PlaySound("elem_3.wav", true);
                        if (t == 2) SoundPlayer.PlaySound("ult_3.wav", true);
                    }
                }
                for (int i = 0; i < bulletPositions[t].Length; i++)
                {
                    bulletPositions[t][i].Y -= bulletSpeeds[t];
                    if (bulletPositions[t][i].X - boss.Position.X != 0)
                        bulletPositions[t][i].X -= (int)Math.Pow(t + 2, 2) * Math.Sign(bulletPositions[t][i].X - boss.Position.X);
                    if (Math.Abs(bulletPositions[t][i].X - boss.Position.X) <= (int)Math.Pow(t + 2, 2))
                        bulletPositions[t][i].X = boss.Position.X;

                    // Check for boss collision
                    if (boss.Rect.IntersectsWith(new Rectangle(bulletPositions[t][i], new Size(16, 16))))
                    {
                        if (t == 0) elementalEnergy++;
                        else if (t == 1)
                        {
                            elementalEnergy += 450;
                            SoundPlayer.PlaySound("elem_2.wav", true);
                        }
                        else
                        {
                            elementalEnergy += 280;
                            SoundPlayer.PlaySound("ult_2.wav", true);
                        }
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

            // Execute controls
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
                        selectedIndex = 0;
                        SoundPlayer.PlaySound("enter.wav", true);
                        paused = !paused;
                        return LogicExit.Nothing; // Overwrite base handler
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

        private void ClearRadius(int radius)
        {
            for (int i = 0; i < bulletPositionsBoss.Length; i++)
            {
                List<Point> remBullets = bulletPositionsBoss[i].ToList();
                remBullets.RemoveAll(p =>
                    (Math.Abs(p.X - player.Position.X) <= radius ||
                        Math.Abs(p.X - player.Position.X - player.Size.Width) <= radius) &&
                        (Math.Abs(p.Y - player.Position.Y) <= radius ||
                        Math.Abs(p.Y - player.Position.Y - player.Size.Height) <= radius)
                );
                bulletPositionsBoss[i] = remBullets.ToArray();
            }
        }

        public static Vector2 SineDown(Vector2 pos)
        {
            return new Vector2(13.0f * -(float)Math.Sin(pos.Y / 31.4), 3) + pos;
        }

        private Vector2 TightSpiral(Vector2 pos)
        {
            pos -= new Vector2(boss.Position.X, boss.Position.Y);

            Polar polar = new(pos.Length(), (float)Math.Atan2(pos.Y, pos.X));
            polar.radius += 4.2f;
            polar.angle += 0.011415f;

            return new Vector2(boss.Position.X, boss.Position.Y) + new Vector2(polar.radius * (float)Math.Cos(polar.angle), polar.radius * (float)Math.Sin(polar.angle));
        }

        private Vector2 Spread(Vector2 pos)
        {
            Vector2 bpos = new(boss.Position.X, boss.Position.Y);
            pos -= bpos;

            Polar polar = new(pos.Length(), (float)Math.Atan2(pos.Y, pos.X));
            polar.radius += 4 * MathF.Ceiling(32.0f / Vector2.Distance(bpos, pos + bpos));

            return bpos + new Vector2(polar.radius * (float)Math.Cos(polar.angle), polar.radius * (float)Math.Sin(polar.angle));
        }
    }
}
