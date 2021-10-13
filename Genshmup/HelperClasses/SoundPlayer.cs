using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NAudio.Wave;

namespace Genshmup.HelperClasses
{
    public static class SoundPlayer
    {
        private static float _volume = 1.0f;
        private static float _sfxvolume = 1.0f;
        public static float Volume
        {
            get { return _volume; }
            set { ChangeVolume(value, false); }
        }
        public static float SFXVolume
        {
            get { return _sfxvolume; }
            set { ChangeVolume(value, true); }
        }

        private static List<(WasapiOut, string, bool)> audioPlayers = new();

        public static void PlaySound(string name, bool sfx = false)
        {
            WasapiOut ap = new();
            StreamMediaFoundationReader reader = new(ResourceLoader.LoadResource(null, name));
            ap.Init(reader);
            ap.Volume = sfx ? _sfxvolume : _volume;
            ap.Play();
            audioPlayers.Add((ap, name, sfx));
            ap.PlaybackStopped += DeleteAudioPlayer;
        }

        public static void PlaySoundLoop(string name)
        {
            WasapiOut ap = new();
            StreamMediaFoundationReader reader = new(ResourceLoader.LoadResource(null, name));
            ap.Init(reader);
            ap.Volume = _volume;
            ap.Play();
            audioPlayers.Add((ap, name, false));
            ap.PlaybackStopped += RenewLoop;
        }

        private static void ChangeVolume(float vol, bool sfx)
        {
            if (sfx) _sfxvolume = vol;
            else _volume = vol;
            for (int i = 0; i < audioPlayers.Count; i++)
            {
                audioPlayers[i].Item1.Volume = audioPlayers[i].Item3 ? _sfxvolume : _volume;
            }
        }

        private static void RenewLoop(object? sender, EventArgs e)
        {
            if (sender == null) return;
            WasapiOut? ap = sender as WasapiOut;
            if (ap == null) return;
            (WasapiOut, string, bool) tuple = audioPlayers.Find(x => x.Item1 == ap);
            audioPlayers.Remove(tuple);
            StreamMediaFoundationReader reader = new(ResourceLoader.LoadResource(null, tuple.Item2));
            ap = new WasapiOut();
            ap.Init(reader);
            ap.Volume = _volume;
            ap.Play();
            audioPlayers.Add((ap, tuple.Item2, tuple.Item3));
            ap.PlaybackStopped += DeleteAudioPlayer;
        }

        public static void DisposeAll()
        {
            for (int i = 0; i < audioPlayers.Count; i++)
            {
                audioPlayers[i].Item1.PlaybackStopped -= RenewLoop;
                audioPlayers[i].Item1.PlaybackStopped -= DeleteAudioPlayer;
                audioPlayers[i].Item1.Stop();
                audioPlayers[i].Item1.Dispose();
                audioPlayers.Remove(audioPlayers[i]);
            }
        }

        private static void DeleteAudioPlayer(object? sender, EventArgs e)
        {
            if (sender == null) return;
            WasapiOut? ap = sender as WasapiOut;
            if (ap == null) return;
            ap.Stop();
            ap.Dispose();
            audioPlayers.Remove(audioPlayers.Find(x => x.Item1 == ap));
        }
    }
}
