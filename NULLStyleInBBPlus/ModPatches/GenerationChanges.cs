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
            if (!Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudio")) return false;
            Type editorType = Type.GetType("PlusLevelStudio.Editor.EditorController, PlusLevelStudio");
            if (editorType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) != null) return true;
            Type playType = Type.GetType("PlusLevelStudio.EditorPlayModeManager, PlusLevelStudio");
            if (playType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) != null) return true;
            return false;
        }

        [HarmonyPatch(typeof(LevelBuilder), "StartGenerate")]
        [HarmonyPrefix]
        private static void ModifyGenerationParameters(LevelBuilder __instance) {
            if (IsEditor()) return;
            if (!ModManager.NullStyle && !ModManager.DoubleTrouble) return;

            LevelGenerationParameters ld = __instance.ld;

            if (ld.randomEvents != null) {
                var allowedEvents = new List<WeightedRandomEvent>();
                
                foreach (var weightedEvent in ld.randomEvents) {
                    if (weightedEvent.selection == null) continue;

                    bool isAllowed = false;
                    RandomEventType type = weightedEvent.selection.Type;
                    string className = weightedEvent.selection.GetType().Name;

                    if (type == RandomEventType.Fog || 
                        type == RandomEventType.MysteryRoom || 
                        type == RandomEventType.Party) {
                        isAllowed = true;
                    }
                    else if (className == "VotingEvent") {
                        isAllowed = true;
                    }

                    if (isAllowed) {
                        allowedEvents.Add(weightedEvent);
                    }
                }

                ld.randomEvents = allowedEvents;
            }
            
            ld.timeOutEvent = null;

            var nullNpc = ModManager.m.Get<NullNPC>("NULL");
            var glitchNpc = ModManager.m.Get<NullNPC>("NULLGLITCH");

            if (nullNpc != null && glitchNpc != null) {
                List<NPC> forcedList = new List<NPC>();
                if (ld.forcedNpcs != null) forcedList.AddRange(ld.forcedNpcs);

                if (ModManager.DoubleTrouble) {
                    ld.potentialBaldis = new WeightedNPC[0];

                    if (!forcedList.Exists(x => x.name == "NULL")) {
                        forcedList.Add(nullNpc);
                    }

                    if (!forcedList.Exists(x => x.name == "NULLGLITCH")) {
                        forcedList.Add(glitchNpc);
                    }
                }
                else if (ModManager.NullStyle) {
                    var target = ModManager.GlitchStyle ? glitchNpc : nullNpc;
                    ld.potentialBaldis = new WeightedNPC[] { new WeightedNPC() { selection = target, weight = 100 } };
                    
                    if (!NULL.BasePlugin.characters.Value) {
                        forcedList.Clear();
                    }
                }
                
                ld.forcedNpcs = forcedList.ToArray();
            }

            if (ld.standardHallBuilders != null) {
                var filtered = new List<RandomHallBuilder>();
                foreach (var b in ld.standardHallBuilders) {
                    if (b.selectable != null) {
                        string name = b.selectable.name.ToLower();
                        if (name.Contains("swing") || name.Contains("gate")) continue;
                        filtered.Add(b);
                    }
                }
                ld.standardHallBuilders = filtered.ToArray();
            }

            if (ld.specialHallBuilders != null) {
                var filtered = new List<WeightedObjectBuilder>();
                foreach (var b in ld.specialHallBuilders) {
                    if (b.selection != null) {
                        string name = b.selection.name.ToLower();
                        if (name.Contains("coin") || name.Contains("lock") || name.Contains("pay") || name.Contains("button")) continue;
                        filtered.Add(b);
                    }
                }
                ld.specialHallBuilders = filtered.ToArray();
            }
            
            ld.forcedSpecialHallBuilders = new ObjectBuilder[0];
        }

        [HarmonyPatch(typeof(LevelBuilder), "LoadRoom", new Type[] { typeof(RoomAsset), typeof(IntVector2), typeof(IntVector2), typeof(Direction), typeof(bool), typeof(Texture2D), typeof(Texture2D), typeof(Texture2D) })]
        [HarmonyPostfix]
        private static void ReplaceFacultyDoors(LevelBuilder __instance, RoomController __result) {
            if (IsEditor() || !ModManager.NullStyle || __result == null) return;

            if (__result.doorPre != null && __result.doorPre.GetComponent<FacultyOnlyDoor>() != null) {
                if (__instance.nullDoorPre != null) __result.doorPre = __instance.nullDoorPre;
                else {
                    var stdDoor = Resources.FindObjectsOfTypeAll<StandardDoor>().FirstOrDefault(x => x.name == "ClassDoor_Standard");
                    if (stdDoor != null) __result.doorPre = stdDoor;
                }
            }
        }

        [HarmonyPatch(typeof(EnvironmentController), "AddEvent")]
        [HarmonyPrefix]
        static bool BlockAllEvents(RandomEvent randomEvent) {
            if (IsEditor()) return true;
            
            if (ModManager.NullStyle) {
                if (randomEvent.Type == RandomEventType.Fog || 
                    randomEvent.Type == RandomEventType.MysteryRoom || 
                    randomEvent.Type == RandomEventType.Party) {
                    return true;
                }
                
                if (randomEvent.GetType().Name == "VotingEvent") {
                    return true;
                }

                return false;
            }
            
            return true;
        }
    }
}