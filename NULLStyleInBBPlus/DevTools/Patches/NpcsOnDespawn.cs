using DevTools.Extensions;
using HarmonyLib;
using MTM101BaldAPI.Reflection;
using System;
using System.Collections.Generic;
using static UnityEngine.Object;

namespace DevTools.Patches
{

    [HarmonyPatch]
    internal class NPCOnDespawn
    {
        [HarmonyPatch(typeof(NPC), "Despawn")]
        [HarmonyPrefix]
        private static void Despawn(NPC __instance) {
            var type = __instance.GetType();
            if (type == typeof(Beans))
            {
                var gum = ((Beans)__instance).gum;
                gum.Reset(null);
                Destroy(gum.gameObject);
                return;
            }
            if (type == typeof(Cumulo))
            {
                ((AudioManager)__instance.ReflectionGetVariable("audMan")).FlushQueue(true);
                Destroy(((BeltManager)__instance.ReflectionGetVariable("windManager")).gameObject);
                return;
            }
            if (type == typeof(LookAtGuy))
            {
                __instance.ec.RemoveFog(((LookAtGuy)__instance).fog);
                ((LookAtGuy)__instance).FreezeNPCs(false);
                return;
            }
            if (type == typeof(NoLateTeacher))
            {
                ((NoLateIcon)__instance.ReflectionGetVariable("mapIcon"))?.gameObject.SetActive(false);
                ((PlayerManager)__instance.ReflectionGetVariable("targetedPlayer"))?.Am.moveMods.Remove((MovementModifier)__instance.ReflectionGetVariable("moveMod"));
                return;
            }
            if (type == typeof(Playtime))
            {
                var rope = (Jumprope)__instance.ReflectionGetVariable("currentJumprope");
                if (rope)
                    rope.Destroy();
                return;
            }

        }

        [HarmonyPatch(typeof(Chalkboard), "Update")]
        [HarmonyPrefix]
        private static bool DestroyIfRequired(Chalkboard __instance, ref RoomController ___room, ChalkFace ___chalkFace) {
            if (___chalkFace == null)
            {
                ((List<RoomFunction>)___room.functions.ReflectionGetVariable("functions")).Remove(__instance);
                Destroy(__instance.gameObject);
                return false;
            }
            return true;
        }
        [HarmonyPatch(typeof(FirstPrize_Active), "Update")]
        [HarmonyPrefix]
        private static bool WhenGetDestroyed(FirstPrize_Active __instance, FirstPrize ___firstPrize, ref PlayerManager ___currentPlayer, MoveModsManager ___moveModsMan) {
            if (___firstPrize == null)
            {
                ___moveModsMan.RemoveAll();
                if (___currentPlayer != null)
                {
                    PlayerManager playerManager = ___currentPlayer;
                    playerManager.onPlayerTeleport = (PlayerManager.PlayerTeleportedFunction)Delegate.Remove(playerManager.onPlayerTeleport, new PlayerManager.PlayerTeleportedFunction(__instance.PlayerTeleported));
                    ___currentPlayer = null;
                }
                return false;
            }

            return true;
        }
    }
}