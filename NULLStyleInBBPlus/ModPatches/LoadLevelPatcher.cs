using DevTools;
using DevTools.Extensions;
using HarmonyLib;
using MTM101BaldAPI;
using NULL.Manager;
using static NULL.Manager.ModManager;
using static System.Text.RegularExpressions.Regex;

namespace NULL.ModPatches {
    [ConditionalPatchNULL]
    [HarmonyPatch]
    internal class LevelLoaderPatcher {
        [HarmonyPatch(typeof(GameLoader), nameof(GameLoader.LoadLevel))]
        [HarmonyPatch(typeof(BaseGameManager), "LoadSceneObject", new[] { typeof(SceneObject) })]
        [HarmonyPrefix]
        static void LoadLevel(ref SceneObject sceneObject) {
            string GetLevelKey(string name, bool glitchStyle, bool hasCharacters) {
                int start = name.StartsWith("NULL_") ? 5 : 7;
                int end = name.EndsWith("_NoNpcs") ? name.Length - 7 : name.Length;

                string baseName = name.Substring(start, end - start);
                string prefix = glitchStyle ? "GLITCH_" : "NULL_";
                string suffix = hasCharacters ? "" : "_NoNpcs";
                return prefix + baseName + suffix;
            }
            if (sceneObject is null || sceneObject.levelObject is null || !nullLevels.ContainsValue((CustomLevelObject)sceneObject.levelObject))
                return;

            sceneObject.SetLevel(nullLevels[GetLevelKey(sceneObject.levelObject.name, GlitchStyle, BasePlugin.characters.Value)]);

            if (ModManager.DoubleTrouble) {
                sceneObject.levelTitle = "DT" + (sceneObject.levelNo + 1);
            }
            else {
                if (IsMatch(sceneObject.levelTitle, @"^N\d$") && sceneObject.levelObject.name.Contains("GLITCH"))
                    sceneObject.levelTitle = sceneObject.levelTitle.Replace("N", "G");
                else if (IsMatch(sceneObject.levelTitle, @"^G\d$") && sceneObject.levelObject.name.Contains("NULL"))
                    sceneObject.levelTitle = sceneObject.levelTitle.Replace("G", "N");
            }
        }
    }
}