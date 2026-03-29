using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NULL.Manager;
using NULL.NPCs;
using System;
using System.Reflection;
using BepInEx.Bootstrap;

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

        [HarmonyPatch(typeof(CoinDoorBuilder), "Build")]
        [HarmonyPrefix]
        private static bool DisableCoinDoorBuild() {
            if (IsEditor()) return true;
            return !ModManager.NullStyle;
        }

        [HarmonyPatch(typeof(LockdownDoorBuilder), "Build")]
        [HarmonyPrefix]
        private static bool DisableLockdownDoorBuild() {
            if (IsEditor()) return true;
            return !ModManager.NullStyle;
        }

        [HarmonyPatch(typeof(OneWayDoorBuilder), "Build")]
        [HarmonyPrefix]
        private static bool DisableOneWayDoorBuild() {
            if (IsEditor()) return true;
            return !ModManager.NullStyle;
        }

        private static bool IsForbiddenItem(ItemObject item) {
            if (item == null) return false;
            
            string n = item.name;
            return item.itemType == Items.Points ||
                    item.itemType == Items.BusPass ||
                   item.itemType == Items.Apple;
        }

        [HarmonyPatch(typeof(LevelBuilder), "StartGenerate")]
        [HarmonyPrefix]
        private static void ModifyGenerationParameters(LevelBuilder __instance) {
            if (IsEditor()) return;
            if (!ModManager.NullStyle && !ModManager.GlitchStyle && !ModManager.DoubleTrouble) return;

            LevelGenerationParameters ld = __instance.ld;

            if (ld.potentialItems != null) {
                ld.potentialItems = ld.potentialItems.Where(wItem => !IsForbiddenItem(wItem.selection)).ToArray();
            }

            if (ld.items != null) {
                ld.items = ld.items.Where(wItem => !IsForbiddenItem(wItem.selection)).ToArray();
            }

            if (ld.forcedItems != null) {
                ld.forcedItems.RemoveAll(item => IsForbiddenItem(item));
            }

            if (ld.randomEvents != null) {
                ld.randomEvents = ld.randomEvents.Where(ev => {
                    if (ev.selection == null) return false;
                    var type = ev.selection.Type;
                    var name = ev.selection.GetType().Name;
                    return type == RandomEventType.Fog || 
                           type == RandomEventType.MysteryRoom || 
                           type == RandomEventType.Party ||
                           name == "VotingEvent";
                }).ToList();
            }
            ld.timeOutEvent = null;

            var nullNpc = ModManager.m.Get<NullNPC>("NULL");
            var glitchNpc = ModManager.m.Get<NullNPC>("NULLGLITCH");

            List<NPC> forcedList = ld.forcedNpcs != null ? new List<NPC>(ld.forcedNpcs) : new List<NPC>();

            if (ModManager.DoubleTrouble) {
                if (nullNpc != null && !forcedList.Exists(x => x.name == "NULL")) forcedList.Add(nullNpc);
                if (glitchNpc != null && !forcedList.Exists(x => x.name == "NULLGLITCH")) forcedList.Add(glitchNpc);
            }
            else if (ModManager.NullStyle) {
                var target = ModManager.GlitchStyle ? glitchNpc : nullNpc;
                
                if (target != null) {
                    if (!forcedList.Contains(target)) forcedList.Add(target);
                    
                    if (!NULL.BasePlugin.characters.Value) {
                        forcedList.Clear();
                        forcedList.Add(target);
                    }
                } else {
                    Debug.LogError("NULL: Target NPC prefab is null during generation!");
                }
            }
            
            ld.forcedNpcs = forcedList.ToArray();

            if (ld.standardHallBuilders != null) {
                ld.standardHallBuilders = ld.standardHallBuilders
                    .Where(b => b.selectable != null && !IsForbiddenObject(b.selectable))
                    .ToArray();
            }

            if (ld.specialHallBuilders != null) {
                ld.specialHallBuilders = ld.specialHallBuilders
                    .Where(b => b.selection != null && !IsForbiddenObject(b.selection))
                    .ToArray();
            }
            
            ld.forcedSpecialHallBuilders = new ObjectBuilder[0];
        }

        private static bool IsForbiddenObject(UnityEngine.Object obj) {
            if (obj is CoinDoorBuilder || obj is LockdownDoorBuilder || obj is OneWayDoorBuilder) return true;
            string n = obj.name.ToLower();
            return n.Contains("coin") || n.Contains("lockdown") || n.Contains("oneway");
        }

        [HarmonyPatch(typeof(LevelBuilder), "LoadRoom", new Type[] { typeof(RoomAsset), typeof(IntVector2), typeof(IntVector2), typeof(Direction), typeof(bool), typeof(Texture2D), typeof(Texture2D), typeof(Texture2D) })]
        [HarmonyPostfix]
        private static void ReplaceRoomDoors(LevelBuilder __instance, RoomController __result) {
            if (IsEditor() || !ModManager.NullStyle || __result == null) return;

            if (__result.doorPre != null) {
                bool badDoor = __result.doorPre.GetComponent<FacultyOnlyDoor>() != null || 
                               __result.doorPre.GetComponent<Door_SwingingOneWay>() != null;

                if (badDoor) {
                    if (__instance.nullDoorPre != null) {
                        __result.doorPre = __instance.nullDoorPre;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(EnvironmentController), "AddEvent")]
        [HarmonyPrefix]
        static bool BlockAllEvents(RandomEvent randomEvent) {
            if (IsEditor()) return true;
            if (ModManager.NullStyle && !BasePlugin.allEvents.Value) {
                if (randomEvent.Type == RandomEventType.Fog ||
                    randomEvent.Type == RandomEventType.MysteryRoom ||
                    randomEvent.Type == RandomEventType.Party) return true;
                if (randomEvent.GetType().Name == "VotingEvent") return true;
                return false;
            }
            return true;
        }
    }
}