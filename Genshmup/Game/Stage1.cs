using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genshmup.HelperClasses;
using System.Drawing;
using System.Reflection;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Genshmup.Game
{
    public class Stage1 : Stage
    {
        private readonly Player player = new();
        private readonly Boss boss = new();

        private readonly List<Image> renderedList;
        private IEnumerator<Image> rendered;
        readonly Point[] noisePoints = new Point[8];

        private readonly int movementSpeed = 10;
        private readonly int movementSpeedShifting = 2;
        private bool shifting = false;

        private readonly Image Kakbrazeus;
        private readonly Image Ganyu;

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

        private bool gameover = false;
        private int selectedIndex = 0;

        public Stage1()
        {
            Kakbrazeus = Image.FromStream(ResourceLoader.LoadResource(null, "kakbrazeus.png") ?? System.IO.Stream.Null);
            Ganyu = Image.FromStream(ResourceLoader.LoadResource(null, "ganyu.png") ?? System.IO.Stream.Null);
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

            bulletAtlas = Image.FromStream(ResourceLoader.LoadResource(null, "bullets1.png") ?? System.IO.Stream.Null);
            bulletSpeeds = new int[] { 20, 10, 5 };
            bulletDamages = new int[] { 2, 600, 1200 };
            bulletCooldowns = new int[] { 5, 600, 1200 };
            _bulletCooldowns = new int[3];
            Array.Fill(_bulletCooldowns, 0);
            CR = new Rectangle(0, 0, 480, 360);

            renderedList = new();
            rendered = renderedList.GetEnumerator();

            player.Lives = 3;
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
                        ) << 8) + (int)Math.Sqrt(noisePoints
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

        public override void Render(Graphics g)
        {
            try
            {
                // Draw BG
                // g.Clear(Color.Black);
                g.DrawImage(rendered.Current ?? renderedList.First(), new Rectangle(0, 0, 480, 360));
                if (!rendered.MoveNext()) rendered.Reset();

                // Dialog
                

                // Danmaku
                DanmakuGraphics.RenderAtlas(g, bulletAtlas, bulletElements, bulletPositions);

                g.DrawString($"{_bulletCooldowns[1]}, {_bulletCooldowns[2]}\n{boss.Health}, {player.Lives}", titlefont, Brushes.White, new Point(240, 180), sf);

                DanmakuGraphics.RenderAtlas(g, bulletAtlas, bulletElementsBoss, bulletPositionsBoss);
                
                g.DrawImage(Kakbrazeus, player.Position);
                g.DrawImage(Ganyu, boss.Position);
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
            pres.Add(boss.Position);
            bulletPositionsBoss[type] = pres.ToArray();
        }

        public override LogicExit Logic(string[] events)
        {
            if (gameover) 
            {
                foreach (string eventName in events)
                {
                    switch (eventName)
                    {
                        case "Up":
                            SoundPlayer.PlaySound("select.wav", true);
                            selectedIndex = 0;
                            break;
                        case "Down":
                            SoundPlayer.PlaySound("select.wav", true);
                            selectedIndex = 1;
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

            for (int i1 = 0; i1 < bulletPositionsBoss.Length; i1++)
            {
                Point[] pa = bulletPositionsBoss[i1];
                for (int i = 0; i < pa.Length; i++)
                {
                    Point ta = pa[i];
                    Rectangle t = new Rectangle(ta, new Size(16, 16));
                    if (player.Rect.IntersectsWith(t))
                    {
                        player.Lives--;
                        List<Point> bpb = bulletPositionsBoss[i1].ToList();
                        bpb.Remove(pa[i]);
                        bulletPositionsBoss[i1] = bpb.ToArray();
                    }
                }
            }

            if (player.Lives == 0)
            {
                SoundPlayer.PlaySound("stage_failed.wav", true);
                gameover = true;
            }

            if (new Random().Next(0, 20) == 0)
            {
                boss.Position = new Point(boss.Position.X + new Random().Next(-10, 10), boss.Position.Y + new Random().Next(-10, 10));
                BossShoot(0);
                boss.Bound(CR);
            }
            if (boss.Health <= 0)
            {
                SoundPlayer.PlaySound("boss_death.wav", true);
                _nextScreen = 2;
                return LogicExit.ScreenChange;
            }
            for (int t = 0; t < bulletPositionsBoss.Length; t++)
            {
                for (int i = 0; i < bulletPositionsBoss[t].Length; i++)
                {
                    bulletPositionsBoss[t][i].Y += 5;
                }
            }

            for (int t = 0; t < bulletPositions.Length; t++)
            {
                if (_bulletCooldowns[t] < bulletCooldowns[t])
                    _bulletCooldowns[t]++;
                for (int i = 0; i < bulletPositions[t].Length; i++)
                {
                    bulletPositions[t][i].Y -= bulletSpeeds[t];
                    if (t >= 1) bulletPositions[t][i].X -= (int)Math.Pow(t, 3) * (bulletPositions[t][i].X - boss.Position.X) / Math.Abs(bulletPositions[t][i].X - boss.Position.X);
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
    }
}
