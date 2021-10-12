using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.SimpleAudioPlayer;

namespace Genshmup.HelperClasses
{
    public static class SoundPlayer
    {
        private static List<ISimpleAudioPlayer> audioPlayers = new();
        public static void PlaySound(string name)
        {
            ISimpleAudioPlayer ap = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
            ap.Load(ResourceLoader.LoadResource(null, name));
            ap.Play();
            ap.Loop = false;
            audioPlayers.Add(ap);
            ap.PlaybackEnded += DeleteAudioPlayer;
        }

        public static void PlaySoundLoop(string name)
        {
            ISimpleAudioPlayer ap = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
            ap.Load(ResourceLoader.LoadResource(null, name));
            ap.Play();
            ap.Loop = true;
            audioPlayers.Add(ap);
        }

        public static void DisposeAll()
        {
            for (int i = 0; i < audioPlayers.Count; i++)
            {
                audioPlayers[i].Stop();
                audioPlayers[i].Dispose();
                audioPlayers.Remove(audioPlayers[i]);
            }
        }

        private static void DeleteAudioPlayer(object? sender, EventArgs e)
        {
            if (sender == null) return;
            ISimpleAudioPlayer? ap = sender as ISimpleAudioPlayer;
            if (ap == null) return;
            ap.Stop();
            ap.Dispose();
            audioPlayers.Remove(ap);
        }
    }
}
