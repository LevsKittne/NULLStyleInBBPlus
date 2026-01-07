using System.Collections.Generic;
using HarmonyLib;

namespace DevTools.Patches {
    [HarmonyPatch(typeof(AudioManager))]
    public class AudioManagerPatcher {
        public static List<string> disabledSounds = new List<string>();

        [HarmonyPatch(typeof(AudioManager), "QueueAudio", new[] { typeof(SoundObject), typeof(bool) })]
        static bool Prefix(SoundObject file) => file != null && !disabledSounds.Contains(file.ToString());

        [HarmonyPatch(typeof(BaldiTV), nameof(BaldiTV.Speak))]
        [HarmonyPrefix]
        static bool NoBaldiTV(SoundObject sound) => sound != null && !disabledSounds.Contains(sound.ToString());
    }
}