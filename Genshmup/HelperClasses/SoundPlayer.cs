using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace Genshmup.HelperClasses
{
    public static class SoundPlayer
    {
        private static float _volume = 1.0f;
        private static float _sfxvolume = 1.0f;
        public static float Volume
        {
            get => _volume;
            set => ChangeVolume(value, false);
        }
        public static float SFXVolume
        {
            get => _sfxvolume;
            set => ChangeVolume(value, true);
        }

        private static readonly List<(WasapiOut, string, bool)> audioPlayers = new();

        public static void PlaySound(string name, bool sfx = false)
        {
            WasapiOut ap = new();
            StreamMediaFoundationReader reader = new(ResourceLoader.LoadResource(null, name));
            ap.Init(reader);
            ap.Play();
            audioPlayers.Add((ap, name, sfx));
            ap.PlaybackStopped += DeleteAudioPlayer;
            ChangeVolume(sfx ? _sfxvolume : _volume, sfx);
        }

        public static void PlaySoundLoop(string name)
        {
            WasapiOut ap = new();
            StreamMediaFoundationReader reader = new(ResourceLoader.LoadResource(null, name));
            ap.Init(reader);
            ap.Play();
            audioPlayers.Add((ap, name, false));
            ap.PlaybackStopped += RenewLoop;
            ChangeVolume(_volume, false);
        }

        private static void ChangeVolume(float vol, bool sfx)
        {
            if (sfx) _sfxvolume = vol;
            else _volume = vol;
            for (int i = 0; i < audioPlayers.Count; i++)
            {
                for (int c = 0; c < audioPlayers[i].Item1.AudioStreamVolume.ChannelCount; c++)
                    audioPlayers[i].Item1.AudioStreamVolume.SetChannelVolume(c, audioPlayers[i].Item3 ? _sfxvolume : _volume);
            }
        }

        private static void RenewLoop(object? sender, EventArgs e)
        {
            if (sender == null) return;
            if (sender is not WasapiOut ap) return;
            (WasapiOut, string, bool) tuple = audioPlayers.Find(x => x.Item1 == ap);
            audioPlayers.Remove(tuple);
            StreamMediaFoundationReader reader = new(ResourceLoader.LoadResource(null, tuple.Item2));
            ap = new WasapiOut();
            ap.Init(reader);
            ap.Play();
            audioPlayers.Add((ap, tuple.Item2, tuple.Item3));
            ap.PlaybackStopped += RenewLoop;
            ChangeVolume(_volume, false);
        }

        public static void DisposeAll()
        {
            // Avoid Clipping and let audio player finish loading if it hasn't already
            for (int i = 0; i < audioPlayers.Count; i++)
            {
                if (audioPlayers[i].Item3) continue;
                for (int c = 0; c < audioPlayers[i].Item1.AudioStreamVolume.ChannelCount; c++)
                    audioPlayers[i].Item1.AudioStreamVolume.SetChannelVolume(c, 0);
            }
            for (int i = 0; i < audioPlayers.Count; i++)
            {
                if (audioPlayers[i].Item3) continue; // SFX can play out
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
            if (sender is not WasapiOut ap) return;
            ap.Stop();
            ap.Dispose();
            audioPlayers.Remove(audioPlayers.Find(x => x.Item1 == ap));
        }
    }
}
