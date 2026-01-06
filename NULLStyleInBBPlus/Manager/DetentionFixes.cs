using HarmonyLib;
using NULL.Manager;
using UnityEngine;
using System.Linq;
using DevTools;

namespace NULL.ModPatches.Fixes {
    [ConditionalPatchNULL]
    [HarmonyPatch]
    internal class DetentionFixes {
        [HarmonyPatch(typeof(BaseGameManager), "Initialize")]
        [HarmonyPrefix]
        private static void DisableAllTimers() {
            if (!ModManager.NullStyle) return;
            string[] targetNames = new string[] {
                "DetentionTimer",
                "DigitalClock",
                "DigitalClock_1"
            };
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var obj in allObjects) {
                if (targetNames.Contains(obj.name)) {
                    obj.SetActive(false);
                }
            }
            var activeClocks = Object.FindObjectsOfType<DigitalClock>(true);
            foreach (var clock in activeClocks) {
                clock.gameObject.SetActive(false);
            }
        }
    }
}