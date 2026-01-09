using HarmonyLib;
using NULL.Manager;
using System;
using System.Reflection;
using UnityEngine;

namespace NULL.Manager.CompatibilityModule {
    public static class CriminalPackCompat {
        public static void Init(Harmony harmony) {
            try {
                Type cpPatchType = AccessTools.TypeByName("CriminalPack.Patches.NoBringingBagAcrossFloorPatch");
                
                if (cpPatchType != null) {
                    MethodInfo originalMethod = AccessTools.Method(cpPatchType, "Prefix");
                    
                    if (originalMethod != null) {
                        harmony.Patch(originalMethod, prefix: new HarmonyMethod(typeof(CriminalPackCompat), nameof(SkipBagLogic)));
                        Debug.Log("NULL: Applied fix for Criminal Pack crash.");
                    }
                }
            }
            catch (Exception e) {
                Debug.LogWarning($"NULL: Failed to apply Criminal Pack compat: {e.Message}");
            }
        }

        public static bool SkipBagLogic() {
            if (ModManager.NullStyle) {
                return false;
            }
            return true;
        }
    }
}