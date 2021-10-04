using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genshmup.HelperClasses
{
    public class Screen : IDisposable
    {
        public int NextScreen { get => _nextScreen; }
        protected int _nextScreen;

        public virtual void Render(Graphics g)
        {
            g.Clear(Color.Black);
            g.DrawString("Something went wrong.", new Font(new FontFamily("Arial"), 12.0f), Brushes.White, new Point(0, 0));
        }

        public virtual LogicExit Logic(string[] events) 
        {
            foreach (string s in events)
            {
                switch (s)
                {
                    case "close": return LogicExit.CloseApplication;
                }
            }
            return 0;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
