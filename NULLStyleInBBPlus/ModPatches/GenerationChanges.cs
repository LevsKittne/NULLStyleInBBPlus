using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using DevTools;
using NULL.Manager;
using NULL.NPCs;
using BepInEx.Bootstrap;
using System.Reflection;

namespace NULL.ModPatches
{
    [HarmonyPatch]
    internal class GenerationChanges
    {

        static bool IsEditor() {
            if (!Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudio")) return false;

            Type editorType = Type.GetType("PlusLevelStudio.Editor.EditorController, PlusLevelStudio");
            if (editorType != null)
            {
                var val = editorType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (val != null) return true;
            }

            Type playType = Type.GetType("PlusLevelStudio.EditorPlayModeManager, PlusLevelStudio");
            if (playType != null)
            {
                var val = playType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (val != null) return true;
            }

            return false;
        }

        [HarmonyPatch(typeof(LevelGenerator), "StartGenerate")]
        [HarmonyPrefix]
        private static void ModifyGenerationParameters(LevelGenerator __instance) {
            if (IsEditor()) return;

            if (!ModManager.NullStyle && !ModManager.DoubleTrouble) return;

            try
            {
                LevelGenerationParameters ld = __instance.ld;

                if (ld.randomEvents != null)
                    ld.randomEvents = new List<WeightedRandomEvent>();

                ld.timeOutEvent = null;

                ld.specialHallBuilders = new WeightedObjectBuilder[0];
                ld.forcedSpecialHallBuilders = new ObjectBuilder[0];
                ld.potentialStructures = new WeightedStructureWithParameters[0];
                ld.forcedStructures = new StructureWithParameters[0];

                var nullNpc = ModManager.m.Get<NullNPC>("NULL");
                var glitchNpc = ModManager.m.Get<NullNPC>("NULLGLITCH");

                if (nullNpc == null) Debug.LogError("NULL NPC not found in AssetManager!");
                if (glitchNpc == null) Debug.LogError("NULLGLITCH NPC not found in AssetManager!");

                if (ModManager.DoubleTrouble)
                {
                    if (nullNpc != null)
                    {
                        ld.potentialBaldis = new WeightedNPC[] {
                            new WeightedNPC() { selection = nullNpc, weight = 100 }
                        };
                    }
                    else
                    {
                        ld.potentialBaldis = new WeightedNPC[0];
                    }

                    var forcedList = new List<NPC>();
                    if (glitchNpc != null) forcedList.Add(glitchNpc);
                    ld.forcedNpcs = forcedList.ToArray();
                }
                else if (ModManager.NullStyle)
                {
                    var targetNpc = ModManager.GlitchStyle ? glitchNpc : nullNpc;

                    if (targetNpc != null)
                    {
                        ld.potentialBaldis = new WeightedNPC[] {
                            new WeightedNPC() { selection = targetNpc, weight = 100 }
                        };
                    }
                    ld.forcedNpcs = new NPC[0];
                }

                if (ld.standardHallBuilders != null)
                {
                    var filteredBuilders = new List<RandomHallBuilder>();
                    foreach (var builder in ld.standardHallBuilders)
                    {
                        if (builder.selectable != null)
                        {
                            string name = builder.selectable.name.ToLower();
                            if (!name.Contains("door") && !name.Contains("gate") && !name.Contains("lock") && !name.Contains("coin"))
                            {
                                filteredBuilders.Add(builder);
                            }
                        }
                    }
                    ld.standardHallBuilders = filteredBuilders.ToArray();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error in GenerationChanges: " + e.Message);
                Debug.LogException(e);
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
            if (IsEditor()) return;
            if (!ModManager.NullStyle || __result == null) return;

            try
            {
                if (__result.doorPre != null && __result.doorPre.GetComponent<FacultyOnlyDoor>() != null)
                {
                    if (__instance.nullDoorPre != null)
                    {
                        __result.doorPre = __instance.nullDoorPre;
                    }
                    else
                    {
                        var stdDoor = Resources.FindObjectsOfTypeAll<StandardDoor>()
                            .FirstOrDefault(x => x.name == "ClassDoor_Standard");

                        if (stdDoor != null)
                        {
                            __result.doorPre = stdDoor;
                        }
                    }
                }
            }
            catch { }
        }

        [HarmonyPatch(typeof(EnvironmentController), "AddEvent")]
        [HarmonyPrefix]
        static bool BlockAllEvents(RandomEvent randomEvent) {
            if (IsEditor()) return true;
            if (ModManager.NullStyle) return false;
            return true;
        }
    }
}