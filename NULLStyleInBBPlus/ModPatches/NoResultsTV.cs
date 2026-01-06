using HarmonyLib;
using MTM101BaldAPI.Reflection;
using UnityEngine;

namespace NULL.ModPatches {
    [HarmonyPatch(typeof(ElevatorScreen))]
    internal class NoResultsTV {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void DisableTVObject(ElevatorScreen __instance) {
            if (!BasePlugin.disableResultsTV.Value) return;

            var screen = (MonoBehaviour)__instance.ReflectionGetVariable("bigScreen");
            if (screen != null) {
                screen.gameObject.SetActive(false);
            }
        }

        [HarmonyPatch("ShowResults")]
        [HarmonyPrefix]
        static bool SkipResultsAnimation() {
            if (!BasePlugin.disableResultsTV.Value) return true;
            return false;
        }
    }
}