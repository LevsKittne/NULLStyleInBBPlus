using DevTools;
using HarmonyLib;
using MTM101BaldAPI.Reflection;
using NULL.NPCs;
using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using static DevTools.ExtraVariables;
using NULL.Content;

namespace NULL.ModPatches.Fixes {
    [ConditionalPatchNULL]
    [HarmonyPatch]
    internal class MinorFixes {
        [HarmonyPatch(typeof(Baldi), "Praise")]
        [HarmonyPrefix]
        static bool NoPraise(Baldi __instance) => !(__instance is NullNPC);

        [HarmonyPatch(typeof(Baldi), "TakeApple")]
        [HarmonyPrefix]
        static bool NoApple(Baldi __instance) => !(__instance is NullNPC);

        [HarmonyPatch(typeof(ElevatorScreen), nameof(ElevatorScreen.UpdateFloorDisplay))]
        [HarmonyPostfix]
        static void SetNormalSizeOfFloorText(TMP_Text ___floorText) {
            if (Core.sceneObject.levelTitle.Length < 4) return;

            ___floorText.fontSize -= 4;
            ___floorText.autoSizeTextContainer = true;
            ___floorText.rectTransform.anchoredPosition -= Vector2.up * 2;
        }

        [HarmonyPatch(typeof(EnvironmentController), "Awake")]
        [HarmonyPostfix]
        static void AngerNullOnSpawn(EnvironmentController __instance) {
            __instance.angerOnSpawn = true;
            __instance.ReflectionSetVariable("npcSpawnBufferRadius", BasePlugin.characters.Value ? 50f : 40f);
        }

        [HarmonyPatch(typeof(ITM_PrincipalWhistle), "Use")]
        [HarmonyPostfix]
        static void NullWhistleReaction() => NullPlusManager.instance.nullNpc.GetAngry(169f);

        [HarmonyPatch(typeof(CoreGameManager), nameof(CoreGameManager.CloseMap))]
        [HarmonyPostfix]
        static void CloseMapFix(CoreGameManager __instance, bool ___disablePause) {
            if (___disablePause) return;
            __instance.GetHud(0).Hide(false);
        }

        [ConditionalPatchNULL]
        [HarmonyPatch(typeof(BaldiTV))]
        internal class BaldiTVFixes {
            [HarmonyPatch(typeof(BaldiTV), nameof(BaldiTV.AnnounceEvent))]
            [HarmonyPatch(typeof(BaldiTV), nameof(BaldiTV.Speak))]
            [HarmonyPrefix]
            static bool NoEventsAnnounces(BaldiTV __instance, bool ___busy, SoundObject sound) {
                if (sound == null || (sound != null && sound.name.Contains("BAL_AllNotebooks"))) return false;
                if (!___busy) {
                    __instance.ReflectionInvoke("QueueEnumerator", new object[] { __instance.ReflectionInvoke("Exclamation", new object[] { 2.5f }) });
                }
                __instance.ReflectionInvoke("QueueEnumerator", new object[] { __instance.ReflectionInvoke("Static", new object[] { 5f }) });
                return false;
            }

            [HarmonyPatch("Static", MethodType.Enumerator)]
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> SetNullOnTV(IEnumerable<CodeInstruction> instructions) {
                var codes = new List<CodeInstruction>(instructions);

                for (int i = 0; i < codes.Count - 3; i++) {
                    if (codes[i].opcode == OpCodes.Ldloc_1 &&
                        codes[i + 1].opcode == OpCodes.Ldfld && codes[i + 1].operand.ToString().Contains("staticObject") &&
                        codes[i + 2].opcode == OpCodes.Ldc_I4_1 &&
                        codes[i + 3].opcode == OpCodes.Callvirt && codes[i + 3].operand.ToString().Contains("SetActive")) {
                        var newInsts = new List<CodeInstruction> {
                            new CodeInstruction(OpCodes.Ldloc_1),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BaldiTV), "baldiImage")),
                            new CodeInstruction(OpCodes.Ldc_I4_1),
                            new CodeInstruction(OpCodes.Call, AccessTools.PropertySetter(typeof(Behaviour), nameof(Behaviour.enabled)))
                        };

                        codes.InsertRange(i + 4, newInsts);
                        break;
                    }
                }
                return codes;
            }
        }
    }
}