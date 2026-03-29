using DevTools;
using HarmonyLib;
using MTM101BaldAPI.Reflection;
using NULL.Content;
using NULL.NPCs;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace NULL.Manager.CompatibilityModule {
    [CompatPatchBBTimes]
    [HarmonyPatch]
    internal class BBTimesCompat {
        internal static float _fixedAnger;
        internal static Coroutine angerCoroutine;

        public class CompatPatchBBTimes : ConditionalPatchNULL {
            public override bool ShouldPatch() => base.ShouldPatch() && Plugins.IsTimes;
        }

        [HarmonyPatch]
        static class BBTimesRedBlocker {
            static MethodBase TargetMethod() => 
                AccessTools.Method("BBTimes.ModPatches.EnvironmentPatches.MainGameManagerPatches:REDAnimation");
            [HarmonyPrefix]
            static bool Prefix() => !ModManager.NullStyle;
            static bool Prepare() => TargetMethod() != null;
        }

        [HarmonyPatch]
        static class BBTimesAngerBlocker {
            static MethodBase TargetMethod() => 
                AccessTools.Method("BBTimes.ModPatches.EnvironmentPatches.MainGameManagerPatches:BaldiAngerPhase");
            [HarmonyPrefix]
            static bool Prefix() => !ModManager.NullStyle;
            static bool Prepare() => TargetMethod() != null;
        }

        [HarmonyPatch(typeof(MonoBehaviour), nameof(MonoBehaviour.StartCoroutine), new[] { typeof(IEnumerator) })]
        [HarmonyPostfix]
        static void GetAngerCoroutine(IEnumerator routine, Coroutine __result) {
            if (routine.ToString().Contains("InfiniteAnger"))
                angerCoroutine = __result;
        }

        [HarmonyPatch(typeof(BossManager), nameof(BossManager.StartBossIntro))]
        [HarmonyPrefix]
        static void OnStartBossIntro(BossManager __instance) {
            var allNulls = UnityEngine.Object.FindObjectsOfType<NullNPC>();
            foreach (var n in allNulls) {
                try {
                    if (angerCoroutine != null)
                        n.StopCoroutine(angerCoroutine);
                }
                catch { }
                n.SetAnger(_fixedAnger);
            }
            Singleton<MusicManager>.Instance.StopFile();
        }

        [HarmonyPatch(typeof(ElevatorManager), "PlayerBrokeElevator")]
        [HarmonyPostfix]
        static void StoreAngerBeforeRage(ElevatorManager __instance) {
            if (NullPlusManager.instance == null) return;
            var cgm = Singleton<CoreGameManager>.Instance;
            if (cgm.sceneObject.nextLevel != null && cgm.sceneObject.nextLevel.name == "NULL") {
                int closed = (int)__instance.ReflectionGetVariable("foundOutOfOrderElevators");
                int toClose = __instance.TotalOutOfOrderElevators;
                if (toClose > 0 && closed >= toClose) {
                    if (NullPlusManager.instance.nullNpc != null) {
                        _fixedAnger = (float)NullPlusManager.instance.nullNpc.ReflectionGetVariable("anger");
                    }
                }
            }
        }
    }
}