using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Genshmup.HelperClasses;
using Genshmup.Game;

namespace Genshmup
{
    public partial class MainForm : Form
    {
        Graphics g;

        public readonly static Rectangle ClientRect = new(0, 0, 480, 360);

        private int phase = 0;

        private readonly static Menu menu = new();

        public MainForm()
        {
            InitializeComponent();
            MinimumSize = Size;
            MaximumSize = Size;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            g = CreateGraphics();

            gameTimer.Interval = 15;
            gameTimer.Enabled = true;
        }

        private void Render()
        {
            if (phase == 0)
                menu.Render(g);
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
