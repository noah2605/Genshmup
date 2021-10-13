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
        private readonly Player player = new();

        private readonly Image Kakbrazeus;

        private readonly Font titlefont;
        private readonly StringFormat sf;

        private Rectangle CR;

        private readonly Point[][] bulletPositions;

        private readonly Image bulletAtlas;

        private readonly Rectangle[] bulletElements;

        public Stage1()
        {
            Kakbrazeus = Image.FromStream(ResourceLoader.LoadResource(null, "kakbrazeus.png") ?? System.IO.Stream.Null);
            titlefont = new Font(ResourceLoader.LoadFont(Assembly.GetExecutingAssembly(), "menu.ttf") ?? new FontFamily(GenericFontFamilies.Serif), 36);
            sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            };

            bulletPositions = new Point[6][];
            for (int i = 0; i < bulletPositions.Length; i++)
            {
                bulletPositions[i] = Array.Empty<Point>();
            }
            bulletElements = new Rectangle[6];
            bulletAtlas = Image.FromStream(ResourceLoader.LoadResource(null, "bullets1.png") ?? System.IO.Stream.Null);

            CR = new Rectangle(0, 0, 480, 360);
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

                // Dialog


                // Danmaku
                DanmakuGraphics.RenderAtlas(g, bulletAtlas, bulletElements, bulletPositions);

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
