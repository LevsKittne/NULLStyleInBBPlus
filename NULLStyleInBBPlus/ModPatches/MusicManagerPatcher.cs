using DevTools;
using HarmonyLib;
using MidiPlayerTK;
using NULL.Content;
using NULL.Manager;
using System.Collections.Generic;

namespace NULL.ModPatches {
    [ConditionalPatchNULL]
    [HarmonyPatch(typeof(MusicManager))]
    internal class MusicManagerPatcher {
        [HarmonyPatch("MidiEvent")]
        static void Postfix(MusicManager __instance, List<MPTKEvent> midiEvents) {
            var b = BossManager.Instance;
            if (!b || (b && b.health <= 0)) return;

            for (int i = 0; i < midiEvents.Count; i++) {
                if (midiEvents[i].Command == MPTKCommand.MetaEvent && midiEvents[i].Meta == MPTKMeta.TextEvent) {
                    if (!b.bossTransitionWaiting) {
                        if (midiEvents[i].Info == "Loop") {
                            __instance.MidiPlayer.MPTK_TickCurrent = 0L;
                        }
                    }
                    else {
                        __instance.MidiPlayer.MPTK_TickCurrent = 4992L;
                        b.bossTransitionWaiting = false;
                        __instance.MidiPlayer.MPTK_Loop = false;
                    }
                }
            }
        }

        [HarmonyPatch("PlayMidi", typeof(string), typeof(bool))]
        [HarmonyPrefix]
        static bool StopElevatorSong(string song) {
            if (song.Equals("Elevator") && ExtraVariables.PlayerInElevator && ModManager.NullStyle)
                return false;

            return true;
        }
    }
}