using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using NULL.Manager;
using NULL.NPCs;
using BepInEx.Bootstrap;
using System.Reflection;

namespace NULL.ModPatches {
    [HarmonyPatch]
    internal class GenerationChanges {
        static bool IsEditor() {
            if (!Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudio")) {
                return false;
            }

            Type editorType = Type.GetType("PlusLevelStudio.Editor.EditorController, PlusLevelStudio");
            if (editorType != null) {
                var val = editorType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (val != null) {
                    return true;
                }
            }

            Type playType = Type.GetType("PlusLevelStudio.EditorPlayModeManager, PlusLevelStudio");
            if (playType != null) {
                var val = playType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (val != null) {
                    return true;
                }
            }

            return false;
        }

        [HarmonyPatch(typeof(LevelBuilder), "StartGenerate")]
        [HarmonyPrefix]
        private static void ModifyGenerationParameters(LevelBuilder __instance) {
            if (IsEditor()) {
                return;
            }

            if (!ModManager.NullStyle && !ModManager.DoubleTrouble) {
                return;
            }

            LevelGenerationParameters ld = __instance.ld;

            if (ld.randomEvents != null) {
                ld.randomEvents.Clear();
            }

            ld.timeOutEvent = null;

            var nullNpc = ModManager.m.Get<NullNPC>("NULL");
            var glitchNpc = ModManager.m.Get<NullNPC>("NULLGLITCH");

            if (nullNpc == null || glitchNpc == null) {
                Debug.LogError("NULL: Critical Error - NPC assets not found.");
                return;
            }

            if (ModManager.DoubleTrouble) {
                ld.potentialBaldis = new WeightedNPC[] {
                    new WeightedNPC() { selection = nullNpc, weight = 100 }
                };

                List<NPC> forcedList = new List<NPC>();
                if (ld.forcedNpcs != null) {
                    forcedList.AddRange(ld.forcedNpcs);
                }

                if (!forcedList.Exists(x => x.Character == glitchNpc.Character)) {
                    forcedList.Add(glitchNpc);
                }
                ld.forcedNpcs = forcedList.ToArray();
            }
            else if (ModManager.NullStyle) {
                var targetNpc = ModManager.GlitchStyle ? glitchNpc : nullNpc;

                ld.potentialBaldis = new WeightedNPC[] {
                    new WeightedNPC() { selection = targetNpc, weight = 100 }
                };

                if (!NULL.BasePlugin.characters.Value) {
                    ld.forcedNpcs = new NPC[0];
                }
            }

            if (ld.standardHallBuilders != null) {
                var filteredBuilders = new List<RandomHallBuilder>();
                foreach (var builder in ld.standardHallBuilders) {
                    if (builder.selectable != null) {
                        string name = builder.selectable.name.ToLower();
                        if (!name.Contains("door") && !name.Contains("gate") && !name.Contains("lock") && !name.Contains("coin")) {
                            filteredBuilders.Add(builder);
                        }
                    }
                }
                ld.standardHallBuilders = filteredBuilders.ToArray();
            }
        }

        [HarmonyPatch(typeof(LevelBuilder), "LoadRoom", new Type[] {
            typeof(RoomAsset),
            typeof(IntVector2),
            typeof(IntVector2),
            typeof(Direction),
            typeof(bool),
            typeof(Texture2D),
            typeof(Texture2D),
            typeof(Texture2D)
        })]
        [HarmonyPostfix]
        private static void ReplaceFacultyDoors(LevelBuilder __instance, RoomController __result) {
            if (IsEditor()) {
                return;
            }
            if (!ModManager.NullStyle || __result == null) {
                return;
            }

            if (__result.doorPre != null && __result.doorPre.GetComponent<FacultyOnlyDoor>() != null) {
                if (__instance.nullDoorPre != null) {
                    __result.doorPre = __instance.nullDoorPre;
                }
                else {
                    var stdDoor = Resources.FindObjectsOfTypeAll<StandardDoor>()
                        .FirstOrDefault(x => x.name == "ClassDoor_Standard");

                    if (stdDoor != null) {
                        __result.doorPre = stdDoor;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(EnvironmentController), "AddEvent")]
        [HarmonyPrefix]
        static bool BlockAllEvents(RandomEvent randomEvent) {
            if (IsEditor()) {
                return true;
            }
            if (ModManager.NullStyle) {
                return false;
            }
            return true;
        }
    }
}