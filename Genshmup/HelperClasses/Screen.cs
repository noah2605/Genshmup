using System;
using System.Drawing;

namespace Genshmup.HelperClasses
{
    public class Screen : IDisposable
    {
        public int NextScreen => _nextScreen;
        protected int _nextScreen;

        public virtual void Init()
        {

        }

        public virtual void Render(Graphics g)
        {
            g.Clear(Color.Black);
            g.DrawString("Something went wrong.", new Font(new FontFamily("Arial"), 12.0f), Brushes.White, new Point(0, 0));
        }

        public virtual LogicExit Logic(string[] events)
        {
            foreach (string ev in events)
                switch (ev)
                {
                    case "F4":
                        SoundPlayer.PlaySound("enter.wav", true);
                        return LogicExit.CloseApplication;
                }
            return 0;
        }

        public void Dispose()
        {
            SoundPlayer.DisposeAll();
            GC.SuppressFinalize(this);
        }
    }
}
