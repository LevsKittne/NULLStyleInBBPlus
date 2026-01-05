using HarmonyLib;
using NULL.NPCs;
using MTM101BaldAPI.Reflection;

namespace NULL.ModPatches {
    [HarmonyPatch(typeof(HideableLockerBaldiInteraction), "Trigger")]
    internal class LockerPatch {
        private static bool Prefix(HideableLockerBaldiInteraction __instance, Baldi baldi) {
            if (baldi is NullNPC nullNpc) {
                HideableLocker locker = (HideableLocker)__instance.ReflectionGetVariable("locker");

                if (locker.playerInside) {
                    PlayerManager player = Singleton<CoreGameManager>.Instance.GetPlayer(0);
                    nullNpc.CaughtPlayer(player);
                    return false;
                }
            }
            return true;
        }
    }
}