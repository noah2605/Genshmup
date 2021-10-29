using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Genshmup.HelperClasses
{
    public class Stage : Screen
    {
        protected int keysleep = 20;
        protected int protection = 60;
        protected int shieldType = 0;

        protected Player player = new();
        protected Boss boss = new();

        protected List<Image> renderedList;
        protected IEnumerator<Image> rendered;
        protected Point[] noisePoints = new Point[8];

        protected int movementSpeed = 10;
        protected int movementSpeedShifting = 2;
        protected bool shifting = false;

        protected Image Heart;

        protected Rectangle CR;

        protected Point[][] bulletPositions;
        protected Image bulletAtlas;
        protected Rectangle[] bulletElements;

        protected int[] bulletSpeeds;
        protected int[] bulletDamages;
        protected int[] bulletCooldowns;
        protected int elementalEnergy = 0;
        protected int[] _bulletCooldowns;

        protected Point[][] bulletPositionsBoss;
        protected Rectangle[] bulletElementsBoss;
        protected Point destinationBoss;

        protected bool gameover = false;
        protected bool dialog = true;
        protected bool paused = false;
        protected int selectedIndex = 0;

        protected string dialogString = "";
        protected Dialog parsedDialog;
        protected DialogElement currentElement = new(ElementType.TextLine, "", "");
        protected int condition = 0;

        protected Stage()
        {
            Heart = Image.FromStream(ResourceLoader.LoadResource(null, "heart.png") ?? Stream.Null);
            bulletPositions = new Point[3][];
            for (int i = 0; i < bulletPositions.Length; i++)
                bulletPositions[i] = Array.Empty<Point>();
            bulletPositionsBoss = new Point[3][];
            for (int i = 0; i < bulletPositionsBoss.Length; i++)
                bulletPositionsBoss[i] = Array.Empty<Point>();
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

            _bulletCooldowns = new int[3];
            Array.Fill(_bulletCooldowns, 300);
            CR = new Rectangle(0, 0, 480, 360);

            renderedList = new();
            rendered = renderedList.GetEnumerator();

            player.Lives = 3;

            bulletAtlas = new Bitmap(1, 1);
            bulletSpeeds = new int[3];
            bulletDamages = new int[3];
            bulletCooldowns = new int[3];
            parsedDialog = Dialog.Empty;
        }

        public override void Init()
        {
            SoundPlayer.PlaySound("stage_intro.wav", true);
        }

        public override void Render(Graphics g)
        {
            base.Render(g);
        }

        public override LogicExit Logic(string[] events)
        {
            return base.Logic(events);
        }
    }
}
