using HarmonyLib;
using NULL.Content;

namespace NULL.ModPatches {
    [HarmonyPatch(typeof(PlayerMovement), "Update")]
    internal class PlayerSpeedPatch {
        [HarmonyPrefix]
        private static void ForceBossSpeed(PlayerMovement __instance) {
            if (BossManager.Instance != null && BossManager.Instance.BossActive) {
                if (__instance.pm.playerNumber == 0) {
                    __instance.walkSpeed = BossManager.Instance.currentPlayerSpeed;
                    __instance.runSpeed = BossManager.Instance.currentPlayerSpeed;
                }
            }
        }
    }
}