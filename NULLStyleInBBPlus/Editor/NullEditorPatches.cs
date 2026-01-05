using HarmonyLib;
using PlusLevelStudio.Editor;
using PlusLevelStudio;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlusLevelStudio.Ingame;

namespace NULL.Editor
{
    [System.Serializable]
    public class NullNpcData
    {
        public int health = 10;
        public bool bossFight = true;
    }

    [System.Serializable]
    public class NullDataWrapper
    {
        public List<string> keys = new List<string>();
        public List<NullNpcData> values = new List<NullNpcData>();
    }

    public class NullVisualSettingsComponent : MonoBehaviour, IEditorSettingsable
    {
        public NullNpcData data;
        public NPCPlacement placement;

        public void SettingsClicked() {
            string status = $"HP: {data.health} | Boss: {data.bossFight}";

            Singleton<EditorController>.Instance.CreateUIPopup(
                $"NULL Settings ({status})\nYes = Toggle Boss Mode\nNo = +1 HP (Max 20)",
                () => {
                    data.bossFight = !data.bossFight;
                    if (!data.bossFight) data.health = 5; else data.health = 10;
                    Singleton<EditorController>.Instance.TriggerError($"Saved: Boss={data.bossFight}");
                },
                () => {
                    data.health++;
                    if (data.health > 20) data.health = 1;
                    Singleton<EditorController>.Instance.TriggerError($"Saved: HP={data.health}");
                }
            );
        }
    }

    [HarmonyPatch]
    internal class NullEditorPatches
    {
        public static Dictionary<IntVector2, NullNpcData> activeNullData = new Dictionary<IntVector2, NullNpcData>();

        [HarmonyPatch(typeof(EditorInterface), "AddNPCVisual")]
        [HarmonyPostfix]
        private static void AddSettingsToVisual(string key, GameObject __result) {
            if (key == "NULL" || key == "NULLGLITCH")
            {
                if (__result.GetComponent<Collider>() == null)
                {
                    var col = __result.AddComponent<BoxCollider>();
                    col.size = new Vector3(5, 10, 5);
                    col.isTrigger = true;
                }

                if (__result.GetComponent<NullVisualSettingsComponent>() == null)
                {
                    __result.AddComponent<NullVisualSettingsComponent>();
                }

                __result.layer = 13;

                if (__result.GetComponent<SettingsComponent>() == null)
                {
                    var settings = __result.AddComponent<SettingsComponent>();
                    settings.offset = Vector3.up * 8f;
                }
            }
        }

        [HarmonyPatch(typeof(EditorController), "AddVisual", typeof(IEditorVisualizable))]
        [HarmonyPostfix]
        private static void OnNpcVisualAdded(IEditorVisualizable visualizable, EditorController __instance) {
            if (visualizable is NPCPlacement npc)
            {
                if (npc.npc == "NULL" || npc.npc == "NULLGLITCH")
                {
                    var visual = __instance.GetVisual(npc);
                    if (visual != null)
                    {
                        var settingsComp = visual.GetComponent<NullVisualSettingsComponent>();
                        if (settingsComp != null)
                        {
                            settingsComp.placement = npc;

                            if (activeNullData.ContainsKey(npc.position))
                            {
                                settingsComp.data = activeNullData[npc.position];
                            }
                            else
                            {
                                var newData = new NullNpcData();
                                activeNullData[npc.position] = newData;
                                settingsComp.data = newData;
                            }
                        }

                        var sc = visual.GetComponent<SettingsComponent>();
                        if (sc != null) sc.activateSettingsOn = settingsComp;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(EditorController), "SaveEditorLevelToFile")]
        [HarmonyPrefix]
        private static void SaveNullData(string path, EditorController __instance) {
            var currentPositions = __instance.levelData.npcs
                .Where(x => x.npc == "NULL" || x.npc == "NULLGLITCH")
                .Select(x => x.position).ToList();

            var keysToRemove = activeNullData.Keys.Where(k => !currentPositions.Contains(k)).ToList();
            foreach (var key in keysToRemove) activeNullData.Remove(key);

            NullDataWrapper wrapper = new NullDataWrapper();
            foreach (var kvp in activeNullData)
            {
                wrapper.keys.Add($"{kvp.Key.x},{kvp.Key.z}");
                wrapper.values.Add(kvp.Value);
            }
            
            string json = JsonUtility.ToJson(wrapper, true);
            string dataPath = path + ".null_data";
            File.WriteAllText(dataPath, json);
        }

        [HarmonyPatch(typeof(EditorController), "LoadEditorLevelFromFile")]
        [HarmonyPostfix]
        private static void LoadNullData(string path, bool __result) {
            if (!__result) return;

            activeNullData.Clear();
            string dataPath = path + ".null_data";

            if (File.Exists(dataPath))
            {
                try
                {
                    string json = File.ReadAllText(dataPath);
                    NullDataWrapper wrapper = JsonUtility.FromJson<NullDataWrapper>(json);

                    if (wrapper != null && wrapper.keys != null)
                    {
                        for (int i = 0; i < wrapper.keys.Count; i++)
                        {
                            string[] split = wrapper.keys[i].Split(new char[] { ',' });
                            if (split.Length >= 2)
                            {
                                IntVector2 pos = new IntVector2(int.Parse(split[0]), int.Parse(split[1]));
                                activeNullData[pos] = wrapper.values[i];
                            }
                        }
                    }
                }
                catch { }
            }
        }

        [HarmonyPatch(typeof(EditorController), "AwakeFunction")]
        [HarmonyPostfix]
        private static void ClearDataOnStart() {
            activeNullData.Clear();
        }
    }
}