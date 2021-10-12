using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Genshmup.HelperClasses;
using Genshmup.Game;

namespace Genshmup
{
    public partial class MainForm : Form
    {
        private Graphics g;
        private BufferedGraphics buffer;

        private int phase = 0;

        private HelperClasses.Screen[] screens = new HelperClasses.Screen[5];

        private List<string> eventBuffer = new List<string>();

        public MainForm()
        {
            InitializeComponent();
            MinimumSize = new Size(96, 72);

            screens = new HelperClasses.Screen[]
            {
                new Menu(),
                new Stage1(),
                new Stage2(),
                new Stage3(),
                new Credits()
            };

            g = CreateGraphics();
            buffer = BufferedGraphicsManager.Current.Allocate(g, ClientRectangle);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            gameTimer.Interval = 15;
            gameTimer.Enabled = true;
        }

        private void Render()
        {
            buffer.Graphics.ScaleTransform(Width/480.0f, Height/360.0f);
            screens[phase].Render(buffer.Graphics);
            buffer.Graphics.ResetTransform();

            buffer.Render(g);
        }

        private bool Logic()
        {
            LogicExit le = screens[phase].Logic(eventBuffer.ToArray());
            if (le == LogicExit.CloseApplication)
            {
                Close();
                return true;
            }
            else if (le == LogicExit.ScreenChange) phase = Math.Max(0, Math.Min(screens[phase].NextScreen, screens.Length));
            eventBuffer.Clear();
            return false;
        }

        private void GameTick(object sender, EventArgs e)
        {
            if (Logic()) return;

            Render();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            string? ev = Enum.GetName(e.KeyCode);
            if (ev != null) eventBuffer.Add(ev);
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                buffer = BufferedGraphicsManager.Current.Allocate(g, ClientRectangle);
            }
            catch { }
        }
    }
}
