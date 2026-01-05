using DevTools;
using HarmonyLib;
using MTM101BaldAPI.Reflection;
using NULL.Content;
using System.Collections;
using UnityEngine;

namespace NULL.Manager.CompatibilityModule {
    [CompatPatchBBTimes]
    [HarmonyPatch]
    internal class BBTimesCompat {
        internal static float _fixedAnger;
        internal static Coroutine angerCoroutine;
        public class CompatPatchBBTimes : ConditionalPatchNULL { public override bool ShouldPatch() => base.ShouldPatch() && Plugins.IsTimes; }

        [HarmonyPatch(typeof(MonoBehaviour), nameof(MonoBehaviour.StartCoroutine), new[] { typeof(IEnumerator) })]
        [HarmonyPostfix]
        static void GetAngerCoroutine(IEnumerator routine, Coroutine __result) {
            if (routine.ToString().Contains("InfiniteAnger")) {
                angerCoroutine = __result;
            }
        }

        [HarmonyPatch(typeof(BossManager), nameof(BossManager.StartBossIntro))]
        [HarmonyPrefix]
        static void OnStartBossIntro(BossManager __instance) {
            var allBosses = __instance.ActiveBosses;
            foreach (var npc in allBosses)
            {
                if (npc == null) continue;

                try {
                    npc.StopCoroutine(angerCoroutine);
                }
                catch (System.Exception ex) {
                    Debug.LogWarning($"[NULL BBTimesCompat] Failed to stop anger coroutine for {npc.name}: {ex.Message}");
                }

                npc.SetAnger(_fixedAnger);
            }

            Singleton<MusicManager>.Instance.StopFile();
        }

        [HarmonyPatch(typeof(NullPlusManager), "ElevatorClosed")]
        [HarmonyPostfix]
        static void StoreAngerBeforeRage(NullPlusManager __instance) {
            int closed = (int)__instance.ReflectionGetVariable("elevatorsClosed");
            int toClose = (int)__instance.ReflectionGetVariable("elevatorsToClose");

            if (closed >= 3 && toClose == 0) {
                _fixedAnger = (float)__instance.nullNpc.ReflectionGetVariable("anger");
            }
        }
    }
}