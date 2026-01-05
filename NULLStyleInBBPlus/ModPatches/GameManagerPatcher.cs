using DevTools;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NULL.ModPatches
{
    [ConditionalPatchNULL]
    [HarmonyPatch]
    internal class GameManagerPatcher
    {
        [HarmonyPatch(typeof(MainGameManager), "CreateHappyBaldi")]
        [HarmonyPrefix]
        static bool CreateHappyBaldi() => false;

        [HarmonyPatch(typeof(MainGameManager), nameof(MainGameManager.BeginPlay))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> BeginPlay(IEnumerable<CodeInstruction> i) {
            List<CodeInstruction> list = new List<CodeInstruction>(i);
            yield return list[0];
            yield return list[1];
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
}