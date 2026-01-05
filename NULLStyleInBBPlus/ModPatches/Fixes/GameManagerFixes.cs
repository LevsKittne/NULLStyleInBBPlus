using DevTools;
using DevTools.Extensions;
using HarmonyLib;
using MTM101BaldAPI.Reflection;
using NULL.NPCs;
using System.Collections.Generic;
using UnityEngine;

namespace NULL.ModPatches.Fixes {
    [ConditionalPatchNULL]
    [HarmonyPatch(typeof(BaseGameManager))]
    internal class GameManagerFixes {
        [HarmonyPatch("ReturnSpawnFinal", MethodType.Enumerator)]
        [HarmonyPrefix]
        static bool FreezeElevators() => !ExtraVariables.freezeElevators;

        [HarmonyPatch("PrepareToLoad")]
        [HarmonyPostfix]
        static void StopMidiBeforeLoad() => Singleton<MusicManager>.Instance.KillMidi();

        [HarmonyPatch(nameof(BaseGameManager.PleaseBaldi))]
        [HarmonyPrefix]
        static bool PleaseNull() {
            var npcs = (List<NPC>)ExtraVariables.ec.ReflectionGetVariable("npcs");
            npcs.FindAll(x => x.GetType() == typeof(NullNPC)).Do(x => x.Pause(Random.Range(6f, 7f)));
            return false;
        }
        [HarmonyPatch(nameof(BaseGameManager.BeginPlay))]
        [HarmonyPostfix]
        static void ShowHud() => ExtraVariables.Core.GetHud(0).Hide(false);

        [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.LoadNextLevel))]
        [HarmonyPrefix]
        static void MidiFix() => Singleton<MusicManager>.Instance.KillMidi();
    }
}