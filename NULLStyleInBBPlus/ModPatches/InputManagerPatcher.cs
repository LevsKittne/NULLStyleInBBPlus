using HarmonyLib;
using NULL.Content;

namespace NULL.ModPatches {
    [HarmonyPatch(typeof(InputManager), "GetDigitalInput")]
    internal class InputManagerPatch {
        static bool Prefix(string id, ref bool __result) {
            if (BossManager.Instance != null && BossManager.Instance.BossActive) {
                if (id == "Map" || id == "MapPlus") {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }
}