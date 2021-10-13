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
        private readonly Graphics g;
        private BufferedGraphics buffer;

        private int phase = 0;

        private readonly HelperClasses.Screen[] screens = new HelperClasses.Screen[5];

        private readonly List<string> eventBuffer = new();

        public MainForm()
        {
            InitializeComponent();
            MinimumSize = Size;
            MaximumSize = Size;

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

            screens[0].Init();
        }

        private void Render()
        {
            screens[phase].Render(buffer.Graphics);

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
            else if (le == LogicExit.ScreenChange)
            {
                screens[phase].Dispose();
                phase = Math.Max(0, Math.Min(screens[phase].NextScreen, screens.Length));
                screens[phase].Init();
            }
            if (phase == 0) eventBuffer.Clear(); // BIOS Input for Menu
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
            if (ev == null || eventBuffer.Contains(ev) && phase == 0) return;
            eventBuffer.Add(ev);
        }
        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            string? ev = Enum.GetName(e.KeyCode);
            if (ev != null) eventBuffer.RemoveAll(i => i == ev);
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                buffer = BufferedGraphicsManager.Current.Allocate(g, ClientRectangle);
            }
            catch { }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (HelperClasses.Screen s in screens)
            {
                s.Dispose();
            }
        }
    }
}
