using MidiPlayerTK;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using UnityEngine;
using MTM101BaldAPI.Reflection;
using static NULL.Manager.ModManager;

namespace DevTools.Extensions
{
    public static class AudioManagerExtensions
    {
        static List<fluid_voice> heldVoices = new List<fluid_voice>();

        public static void Play(this AudioSource source, string name, bool loop = false) {
            source.clip = m.Get<SoundObject>(name).soundClip;
            source.loop = loop;
            source.Play();
        }

        public static void Play(this AudioManager audMan, string name, bool loop = false) {
            audMan.FlushQueue(true);
            audMan.QueueAudio(name, loop);
        }

        public static void QueueAudio(this AudioManager audMan, string name, bool loop = false) {
            audMan.QueueAudio(m.Get<SoundObject>(name));
            audMan.SetLoop(loop);
        }

        public static void PlaySingle(this AudioManager man, string name) => man.PlaySingle(m.Get<SoundObject>(name));

        public static void QueueFile(this MusicManager mm, string name, bool loop = false) => mm.QueueFile(m.Get<LoopingSoundObject>(name), loop);

        public static void KillMidi(this MusicManager mm) {
            mm.HangMidi(false);
            mm.StopMidi();
            mm.MidiPlayer.MPTK_StopSynth();
        }

        public static void HangMidi(this MusicManager mm, bool stop, bool keepDrums = false) {
            var voices = (List<fluid_voice>)mm.MidiPlayer.ReflectionGetVariable("ActiveVoices");

            foreach (fluid_voice fluid_voice in heldVoices)
            {
                if (fluid_voice != null)
                {
                    fluid_voice.DurationTick = 0L;
                }
            }
            heldVoices.Clear();
            for (int i = 0; i < 16; i++)
            {
                if (i != 9)
                {
                    mm.MidiPlayer.MPTK_ChannelEnableSet(i, !stop);
                }
                else
                {
                    mm.MidiPlayer.MPTK_ChannelEnableSet(i, !stop || keepDrums);
                }
            }
            if (stop && voices != null)
            {
                foreach (fluid_voice fluid_voice2 in voices)
                {
                    fluid_voice2.DurationTick = -1L;
                    heldVoices.Add(fluid_voice2);
                }
            }
        }

        public static void PlaySoundAtPoint(SoundObject sound, Vector3 point, float minDistance = 25, float maxDistance = 50, float volume = 1) {
            var audman = new GameObject("PlaySoundAtPoint").CreateAudioManager(minDistance, maxDistance);
            audman.audioDevice.volume = volume;
            audman.transform.position = point;
            audman.PlaySingle(sound);
            Object.Destroy(audman.gameObject, sound.soundClip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
        }

        public static void PlaySoundAtPoint(string sound, Vector3 point, float minDistance = 25, float maxDistance = 50, float volume = 1) => PlaySoundAtPoint(m.Get<SoundObject>(sound), point, minDistance, maxDistance, volume);
    }
}