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

        private readonly static Menu menu = new();
        private readonly static Stage[] stages;

        public MainForm()
        {
            InitializeComponent();
            MinimumSize = Size;
            MaximumSize = Size;

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
            if (phase == 0)
                menu.Render(buffer.Graphics);

            buffer.Render(g);
        }

        private void Logic()
        {
            if (phase == 0)
            {
                LogicExit le = menu.Logic(Array.Empty<string>());
                if (le == LogicExit.CloseApplication) Close();
                else if (le == LogicExit.ScreenChange) phase = menu.NextScreen;
            }
        }

        private void GameTick(object sender, EventArgs e)
        {
            Logic();

            Render();
        }
    }
}
