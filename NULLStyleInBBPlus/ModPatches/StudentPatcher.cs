using DevTools;
using HarmonyLib;
using NULL.Manager;
using UnityEngine;

namespace NULL.ModPatches
{
    [ConditionalPatchNULL]
    [HarmonyPatch(typeof(Structure_StudentSpawner), "SpawnStudents")]
    internal class StudentSpawnerPatch {
        static bool Prefix() {
            return !ModManager.NullStyle;
        }
    }

    [ConditionalPatchNULL]
    [HarmonyPatch(typeof(Student), "Initialize")]
    internal class StudentInitPatch
    {
        static bool Prefix(Student __instance) {
            if (ModManager.NullStyle) {
                Object.Destroy(__instance.gameObject);
                return false;
            }
            return true;
        }
    }
}